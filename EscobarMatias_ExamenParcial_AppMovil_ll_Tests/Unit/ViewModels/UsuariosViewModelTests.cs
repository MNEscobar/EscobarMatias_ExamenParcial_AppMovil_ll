using NSubstitute.ExceptionExtensions;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.ViewModels;

// El ViewModel recibe IUsuarioApiService, IDialogService, INavigationService e IVibracionService; los cuatro son mocks.
// (IConectividadService no aparece acá: lo usa el servicio de API, que se prueba en UsuarioApiServiceTests.)
public class UsuariosViewModelTests
{
    private readonly IUsuarioApiService _api = Substitute.For<IUsuarioApiService>();
    private readonly IDialogService _dialogos = Substitute.For<IDialogService>();
    private readonly INavigationService _navegacion = Substitute.For<INavigationService>();
    private readonly IVibracionService _vibracion = Substitute.For<IVibracionService>();

    private UsuariosViewModel CrearViewModel() => new(_api, _dialogos, _navegacion, _vibracion);

    private static List<UsuarioApi> Usuarios(int cantidad) =>
        Enumerable.Range(1, cantidad).Select(i => new UsuarioApi { Id = i, Nombre = $"Usuario {i}" }).ToList();

    // Configura qué devuelve el servicio de API (que es un mock: no sale a internet).
    private void ApiDevuelve(ApiResult<List<UsuarioApi>> resultado) =>
        _api.ObtenerUsuariosAsync(Arg.Any<CancellationToken>()).Returns(resultado);

    // ---------------------------- Estado inicial ----------------------------
    [Fact]
    public void EstadoInicial_InvitaAPresionarElBoton_SinErrorNiUsuarios()
    {
        // Arrange + Act
        var vm = CrearViewModel();

        // Assert
        Assert.Contains("botón", vm.MensajeEstado);
        Assert.False(vm.HayError);
        Assert.False(vm.EstaCargando);
        Assert.Empty(vm.Usuarios);
    }

    // -------------------------------- Éxito --------------------------------
    [Fact]
    public async Task CargarUsuarios_Exito_LlenaLaListaYMuestraElCodigoHttp()
    {
        // Arrange
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Ok(Usuarios(3), 200));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(3, vm.Usuarios.Count);
        Assert.False(vm.HayError);
        Assert.False(vm.EstaCargando);
        Assert.Contains("HTTP 200", vm.MensajeEstado);
        Assert.Contains("3", vm.MensajeEstado);
    }

    [Fact]
    public async Task CargarUsuarios_UnaSegundaCargaReemplazaLaListaAnterior()
    {
        // Arrange: una primera carga con 3 usuarios...
        var vm = CrearViewModel();
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Ok(Usuarios(3), 200));
        await vm.CargarUsuariosCommand.ExecuteAsync(null);
        // ...y la API pasa a devolver uno solo.
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Ok(Usuarios(1), 200));

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(vm.Usuarios);
    }

    // ------------------------------- Errores -------------------------------
    [Theory]
    [InlineData(TipoErrorApi.SinConexion, null, "conexión")]
    [InlineData(TipoErrorApi.TiempoAgotado, null, "tardó")]
    [InlineData(TipoErrorApi.SolicitudInvalida, 400, "400")]
    [InlineData(TipoErrorApi.NoAutorizado, 401, "401")]
    [InlineData(TipoErrorApi.NoEncontrado, 404, "404")]
    [InlineData(TipoErrorApi.ErrorServidor, 500, "500")]
    [InlineData(TipoErrorApi.RespuestaInvalida, null, "formato")]
    [InlineData(TipoErrorApi.SinDatos, 200, "no devolvió")]
    [InlineData(TipoErrorApi.Desconocido, null, "desconocido")]
    public async Task CargarUsuarios_ConError_MarcaElErrorYMuestraUnMensajeEspecifico(
        TipoErrorApi error, int? codigoHttp, string fragmentoEsperado)
    {
        // Arrange
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Falla(error, codigoHttp));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HayError);
        Assert.Empty(vm.Usuarios);
        Assert.False(vm.EstaCargando);
        Assert.Contains(fragmentoEsperado, vm.MensajeEstado);
    }

    [Fact]
    public async Task CargarUsuarios_CadaTipoDeErrorTieneSuPropioMensaje()
    {
        // Arrange
        var vm = CrearViewModel();
        var tiposDeError = Enum.GetValues<TipoErrorApi>().Where(e => e != TipoErrorApi.Ninguno).ToList();
        var mensajes = new HashSet<string>();

        // Act
        foreach (var error in tiposDeError)
        {
            ApiDevuelve(ApiResult<List<UsuarioApi>>.Falla(error, 500));
            await vm.CargarUsuariosCommand.ExecuteAsync(null);
            mensajes.Add(vm.MensajeEstado);
        }

        // Assert: ningún mensaje se repite entre tipos de error distintos.
        Assert.Equal(tiposDeError.Count, mensajes.Count);
    }

    [Fact]
    public async Task CargarUsuarios_SiElServicioLanzaUnaExcepcion_MuestraUnMensajeGenericoYNoQuedaCargando()
    {
        // Arrange
        _api.ObtenerUsuariosAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HayError);
        Assert.Contains("inesperado", vm.MensajeEstado);
        Assert.False(vm.EstaCargando);
    }

    // ------------------------- Estado durante la carga -------------------------
    [Fact]
    public async Task CargarUsuarios_MientrasCarga_ElBotonQuedaDeshabilitadoYSeInformaElProgreso()
    {
        // Arrange: la API "no responde" hasta que la prueba lo decida.
        var respuestaPendiente = new TaskCompletionSource<ApiResult<List<UsuarioApi>>>();
        _api.ObtenerUsuariosAsync(Arg.Any<CancellationToken>()).Returns(respuestaPendiente.Task);
        var vm = CrearViewModel();

        // Act: se inicia la carga sin esperar a que termine.
        var carga = vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert (durante la carga)
        Assert.True(vm.EstaCargando);
        Assert.False(vm.CargarUsuariosCommand.CanExecute(null));
        Assert.Equal("Cargando usuarios...", vm.MensajeEstado);

        // Act + Assert (al terminar la carga)
        respuestaPendiente.SetResult(ApiResult<List<UsuarioApi>>.Ok(Usuarios(1), 200));
        await carga;
        Assert.False(vm.EstaCargando);
        Assert.True(vm.CargarUsuariosCommand.CanExecute(null));
    }

    // ----------------------------- Ver detalle -----------------------------
    [Fact]
    public async Task VerDetalle_NavegaConElUsuarioSeleccionado()
    {
        // Arrange
        var usuario = new UsuarioApi { Id = 4, Nombre = "Patricia Lebsack" };
        var vm = CrearViewModel();

        // Act
        await vm.VerDetalleCommand.ExecuteAsync(usuario);

        // Assert
        await _navegacion.Received(1).NavegarAAsync(
            RutasNavegacion.UsuarioApiDetalle,
            Arg.Is<IDictionary<string, object>?>(p =>
                p != null && ReferenceEquals(p[RutasNavegacion.ParametroUsuario], usuario)));
    }

    [Fact]
    public async Task VerDetalle_ConNull_NoNavega()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        await vm.VerDetalleCommand.ExecuteAsync(null);

        // Assert
        await _navegacion.DidNotReceive().NavegarAAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>());
    }

    [Fact]
    public async Task VerDetalle_SiLaNavegacionFalla_AvisaAlUsuario()
    {
        // Arrange
        _navegacion.NavegarAAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>())
                   .ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.VerDetalleCommand.ExecuteAsync(new UsuarioApi { Id = 1 });

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
    }

    // ----------------------------- Vibración ante errores -----------------------------
    [Theory]
    [InlineData(TipoErrorApi.SinConexion)]
    [InlineData(TipoErrorApi.TiempoAgotado)]
    [InlineData(TipoErrorApi.SolicitudInvalida)]
    [InlineData(TipoErrorApi.ErrorServidor)]
    [InlineData(TipoErrorApi.RespuestaInvalida)]
    [InlineData(TipoErrorApi.Desconocido)]
    public async Task CargarUsuarios_ConErrorDeApi_VibraUnaVezConLaDuracionCorta(TipoErrorApi error)
    {
        // Arrange
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Falla(error, 500));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        _vibracion.Received(1).Vibrar(UsuariosViewModel.DuracionVibracionError);
    }

    [Fact]
    public async Task CargarUsuarios_ConExcepcionInesperada_Vibra()
    {
        // Arrange
        _api.ObtenerUsuariosAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        _vibracion.Received(1).Vibrar(UsuariosViewModel.DuracionVibracionError);
    }

    [Fact]
    public async Task CargarUsuarios_Exito_NoVibra()
    {
        // Arrange
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Ok(Usuarios(3), 200));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        _vibracion.DidNotReceive().Vibrar(Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task CargarUsuarios_ConDosErroresSeguidos_VibraEnCadaUno()
    {
        // Arrange
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Falla(TipoErrorApi.SinConexion));
        var vm = CrearViewModel();

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        _vibracion.Received(2).Vibrar(UsuariosViewModel.DuracionVibracionError);
    }

    [Fact]
    public async Task CargarUsuarios_ConErrorSeguidoDeExito_SoloVibraEnElError()
    {
        // Arrange: primero falla...
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Falla(TipoErrorApi.ErrorServidor, 500));
        var vm = CrearViewModel();
        await vm.CargarUsuariosCommand.ExecuteAsync(null);
        // ...y después la API responde bien.
        ApiDevuelve(ApiResult<List<UsuarioApi>>.Ok(Usuarios(2), 200));

        // Act
        await vm.CargarUsuariosCommand.ExecuteAsync(null);

        // Assert
        _vibracion.Received(1).Vibrar(Arg.Any<TimeSpan>());
        Assert.False(vm.HayError);
    }

    [Fact]
    public void DuracionVibracionError_EsUnaVibracionCorta()
    {
        // Arrange + Act
        double milisegundos = UsuariosViewModel.DuracionVibracionError.TotalMilliseconds;

        // Assert: más que cero y como máximo medio segundo.
        Assert.InRange(milisegundos, 1, 500);
    }

    [Fact]
    public async Task VerDetalle_SiLaNavegacionFalla_NoVibra()
    {
        // Arrange: la vibración es solo para errores de la API, no para otros problemas.
        _navegacion.NavegarAAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>())
                   .ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.VerDetalleCommand.ExecuteAsync(new UsuarioApi { Id = 1 });

        // Assert
        _vibracion.DidNotReceive().Vibrar(Arg.Any<TimeSpan>());
    }
}