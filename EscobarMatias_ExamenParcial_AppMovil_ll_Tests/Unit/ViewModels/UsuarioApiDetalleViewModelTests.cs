using NSubstitute.ExceptionExtensions;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.ViewModels;

// El ViewModel recibe IUbicacionService, que acá es un mock: no se usa el GPS real ni se piden permisos.
public class UsuarioApiDetalleViewModelTests
{
    private readonly IUbicacionService _ubicacion = Substitute.For<IUbicacionService>();

    private UsuarioApiDetalleViewModel CrearViewModel() => new(_ubicacion);

    // Ubicación del "dispositivo" en las pruebas: Buenos Aires.
    private static readonly Ubicacion BuenosAires = new(-34.6037, -58.3816);

    // Usuario con las coordenadas que devuelve JSONPlaceholder para "Leanne Graham".
    // Distancia real hasta Buenos Aires: ≈ 10 991 km.
    private static UsuarioApi UsuarioConCoordenadas(string latitud = "-37.3159", string longitud = "81.1496") => new()
    {
        Id = 1,
        Nombre = "Leanne Graham",
        Direccion = new DireccionApi { Geo = new GeoApi { Latitud = latitud, Longitud = longitud } }
    };

    private static Dictionary<string, object> Parametros(object valor) =>
        new() { [RutasNavegacion.ParametroUsuario] = valor };

    private void GpsDevuelve(ResultadoUbicacion resultado) =>
        _ubicacion.ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>()).Returns(resultado);

    // ------------------------ Recepción de parámetros ------------------------
    [Fact]
    public void RecibirParametros_ConUnUsuario_LoExponeParaLaPantalla()
    {
        // Arrange
        var vm = CrearViewModel();
        var usuario = new UsuarioApi { Id = 3, Nombre = "Clementine Bauch" };

        // Act
        vm.RecibirParametros(Parametros(usuario));

        // Assert
        Assert.Same(usuario, vm.Usuario);
    }

    [Fact]
    public void RecibirParametros_ConUnObjetoDeOtroTipo_NoCambiaElUsuario()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        vm.RecibirParametros(Parametros("no soy un usuario"));

        // Assert
        Assert.Null(vm.Usuario);
    }

    [Fact]
    public void RecibirParametros_SinLaClaveEsperada_NoCambiaElUsuario()
    {
        // Arrange
        var vm = CrearViewModel();
        var parametros = new Dictionary<string, object> { ["OtraClave"] = new UsuarioApi() };

        // Act
        vm.RecibirParametros(parametros);

        // Assert
        Assert.Null(vm.Usuario);
    }

    [Fact]
    public void EstadoInicial_SinUsuarioNiMensajeDeDistancia()
    {
        // Arrange + Act
        var vm = CrearViewModel();

        // Assert
        Assert.Null(vm.Usuario);
        Assert.False(vm.EstaBuscandoUbicacion);
        Assert.False(vm.HayMensajeDistancia);
        Assert.False(vm.HayErrorDistancia);
        Assert.Null(vm.DistanciaKm);
    }

    [Fact]
    public async Task RecibirParametros_ConOtroUsuario_BorraElResultadoAnterior()
    {
        // Arrange: ya se había calculado una distancia para un usuario.
        GpsDevuelve(ResultadoUbicacion.Ok(BuenosAires));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);
        Assert.NotNull(vm.DistanciaKm);

        // Act
        vm.RecibirParametros(Parametros(new UsuarioApi { Id = 2, Nombre = "Ervin Howell" }));

        // Assert
        Assert.Null(vm.DistanciaKm);
        Assert.False(vm.HayMensajeDistancia);
    }

    // ------------------------ Cálculo de distancia: éxito ------------------------
    [Fact]
    public async Task CalcularDistancia_Exito_MuestraLaDistanciaHastaElUsuario()
    {
        // Arrange
        GpsDevuelve(ResultadoUbicacion.Ok(BuenosAires));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.False(vm.HayErrorDistancia);
        Assert.False(vm.EstaBuscandoUbicacion);
        Assert.NotNull(vm.DistanciaKm);
        Assert.InRange(vm.DistanciaKm!.Value, 10985.0, 10997.0);
        Assert.True(vm.HayMensajeDistancia);
        Assert.Contains("km", vm.MensajeDistancia);
        Assert.Contains("Leanne Graham", vm.MensajeDistancia);
    }

    [Fact]
    public async Task CalcularDistancia_DesdeElMismoLugarDelUsuario_MuestraMenosDeUnKm()
    {
        // Arrange: el dispositivo está exactamente en las coordenadas del usuario.
        GpsDevuelve(ResultadoUbicacion.Ok(new Ubicacion(-37.3159, 81.1496)));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(0.0, vm.DistanciaKm);
        Assert.Contains("menos de 1 km", vm.MensajeDistancia);
    }

    [Fact]
    public async Task CalcularDistancia_PideLaUbicacionUnaSolaVez()
    {
        // Arrange
        GpsDevuelve(ResultadoUbicacion.Ok(BuenosAires));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        await _ubicacion.Received(1).ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>());
    }

    // ------------------------ Cálculo de distancia: fallos del GPS ------------------------
    [Theory]
    [InlineData(EstadoUbicacion.PermisoDenegado, "permiso")]
    [InlineData(EstadoUbicacion.GpsDesactivado, "GPS")]
    [InlineData(EstadoUbicacion.NoSoportado, "no permite")]
    [InlineData(EstadoUbicacion.Error, "No se pudo obtener")]
    public async Task CalcularDistancia_ConFalloDelGps_MuestraUnMensajeClaro(EstadoUbicacion estado, string fragmentoEsperado)
    {
        // Arrange
        GpsDevuelve(ResultadoUbicacion.Falla(estado));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HayErrorDistancia);
        Assert.Null(vm.DistanciaKm);
        Assert.False(vm.EstaBuscandoUbicacion);
        Assert.Contains(fragmentoEsperado, vm.MensajeDistancia);
    }

    [Fact]
    public async Task CalcularDistancia_CadaEstadoDeFalloTieneSuPropioMensaje()
    {
        // Arrange
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));
        var estados = Enum.GetValues<EstadoUbicacion>().Where(e => e != EstadoUbicacion.Exito).ToList();
        var mensajes = new HashSet<string>();

        // Act
        foreach (var estado in estados)
        {
            GpsDevuelve(ResultadoUbicacion.Falla(estado));
            await vm.CalcularDistanciaCommand.ExecuteAsync(null);
            mensajes.Add(vm.MensajeDistancia);
        }

        // Assert
        Assert.Equal(estados.Count, mensajes.Count);
    }

    [Fact]
    public async Task CalcularDistancia_SiElServicioLanzaUnaExcepcion_MuestraUnMensajeGenerico()
    {
        // Arrange
        _ubicacion.ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>())
                  .ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HayErrorDistancia);
        Assert.Contains("inesperado", vm.MensajeDistancia);
        Assert.False(vm.EstaBuscandoUbicacion);
    }

    [Fact]
    public async Task CalcularDistancia_UnaSegundaVezConError_BorraLaDistanciaAnterior()
    {
        // Arrange: un primer intento exitoso...
        GpsDevuelve(ResultadoUbicacion.Ok(BuenosAires));
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);
        Assert.NotNull(vm.DistanciaKm);
        // ...y después el usuario desactiva el GPS.
        GpsDevuelve(ResultadoUbicacion.Falla(EstadoUbicacion.GpsDesactivado));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.Null(vm.DistanciaKm);
        Assert.True(vm.HayErrorDistancia);
    }

    // ------------------------ Estado de carga ------------------------
    [Fact]
    public async Task CalcularDistancia_MientrasBusca_MuestraProgresoYDeshabilitaElBoton()
    {
        // Arrange: el GPS "no responde" hasta que la prueba lo decida.
        var respuestaPendiente = new TaskCompletionSource<ResultadoUbicacion>();
        _ubicacion.ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>()).Returns(respuestaPendiente.Task);
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas()));

        // Act: se inicia la búsqueda sin esperar a que termine.
        var busqueda = vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert (durante la búsqueda)
        Assert.True(vm.EstaBuscandoUbicacion);
        Assert.False(vm.CalcularDistanciaCommand.CanExecute(null));
        Assert.Equal("Obteniendo tu ubicación...", vm.MensajeDistancia);

        // Act + Assert (al terminar la búsqueda)
        respuestaPendiente.SetResult(ResultadoUbicacion.Ok(BuenosAires));
        await busqueda;
        Assert.False(vm.EstaBuscandoUbicacion);
        Assert.True(vm.CalcularDistanciaCommand.CanExecute(null));
        Assert.NotNull(vm.DistanciaKm);
    }

    // ------------------------ Datos del usuario que impiden calcular ------------------------
    [Fact]
    public async Task CalcularDistancia_SinUsuario_NoConsultaElGps()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        await _ubicacion.DidNotReceive().ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>());
        Assert.False(vm.HayMensajeDistancia);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "10")]
    [InlineData("95", "10")]
    public async Task CalcularDistancia_ConUsuarioSinCoordenadasValidas_AvisaYNoConsultaElGps(string latitud, string longitud)
    {
        // Arrange
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(UsuarioConCoordenadas(latitud, longitud)));

        // Act
        await vm.CalcularDistanciaCommand.ExecuteAsync(null);

        // Assert
        Assert.True(vm.HayErrorDistancia);
        Assert.Contains("coordenadas", vm.MensajeDistancia);
        await _ubicacion.DidNotReceive().ObtenerUbicacionActualAsync(Arg.Any<CancellationToken>());
    }
}