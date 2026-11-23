using NSubstitute.ExceptionExtensions;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.ViewModels;

public class ContactoDetalleViewModelTests
{
    private readonly IContactoRepository _repositorio = Substitute.For<IContactoRepository>();
    private readonly IDialogService _dialogos = Substitute.For<IDialogService>();
    private readonly INavigationService _navegacion = Substitute.For<INavigationService>();

    private ContactoDetalleViewModel CrearViewModel() => new(_repositorio, _dialogos, _navegacion);

    // Arma el diccionario de parámetros tal como lo entregaría la navegación.
    private static Dictionary<string, object> Parametros(object valor) =>
        new() { [RutasNavegacion.ParametroContacto] = valor };

    // ------------------------ Recepción de parámetros ------------------------
    [Fact]
    public void RecibirParametros_ConContactoNuevo_TituloNuevoYCamposVacios()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        vm.RecibirParametros(Parametros(new Contacto()));

        // Assert
        Assert.Equal("Nuevo contacto", vm.TituloPagina);
        Assert.Equal(string.Empty, vm.Nombre);
    }

    [Fact]
    public void RecibirParametros_ConContactoExistente_CargaLosCamposYCambiaElTitulo()
    {
        // Arrange
        var vm = CrearViewModel();
        var existente = new Contacto { Id = 5, Nombre = "Ana", Telefono = "342-5123456", Email = "ana@mail.com" };

        // Act
        vm.RecibirParametros(Parametros(existente));

        // Assert
        Assert.Equal("Editar contacto", vm.TituloPagina);
        Assert.Equal("Ana", vm.Nombre);
        Assert.Equal("342-5123456", vm.Telefono);
        Assert.Equal("ana@mail.com", vm.Email);
    }

    [Fact]
    public void RecibirParametros_ConUnObjetoDeOtroTipo_NoCambiaNada()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        vm.RecibirParametros(Parametros("no soy un contacto"));

        // Assert
        Assert.Equal("Nuevo contacto", vm.TituloPagina);
        Assert.Equal(string.Empty, vm.Nombre);
    }

    // ------------------------------ Guardar ------------------------------
    [Fact]
    public async Task Guardar_ConDatosInvalidos_MuestraLosErroresYNoGuardaNiCierra()
    {
        // Arrange
        var vm = CrearViewModel();
        vm.Nombre = "";
        vm.Telefono = "abc";
        vm.Email = "no-es-un-email";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        Assert.NotEqual(string.Empty, vm.ErrorNombre);
        Assert.NotEqual(string.Empty, vm.ErrorTelefono);
        Assert.NotEqual(string.Empty, vm.ErrorEmail);
        await _repositorio.DidNotReceive().InsertarAsync(Arg.Any<Contacto>());
        await _repositorio.DidNotReceive().ActualizarAsync(Arg.Any<Contacto>());
        await _navegacion.DidNotReceive().VolverAsync();
    }

    [Fact]
    public async Task Guardar_ContactoNuevo_InsertaConLosDatosSinEspaciosYCierraElModal()
    {
        // Arrange
        _repositorio.InsertarAsync(Arg.Any<Contacto>()).Returns(true);
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(new Contacto()));
        vm.Nombre = "  Ana García  ";
        vm.Telefono = " 342-5123456 ";
        vm.Email = " ana@mail.com ";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        await _repositorio.Received(1).InsertarAsync(Arg.Is<Contacto>(c =>
            c.Id == 0 && c.Nombre == "Ana García" && c.Telefono == "342-5123456" && c.Email == "ana@mail.com"));
        await _repositorio.DidNotReceive().ActualizarAsync(Arg.Any<Contacto>());
        await _navegacion.Received(1).VolverAsync();
    }

    [Fact]
    public async Task Guardar_ContactoExistente_ActualizaConSuIdYCierraElModal()
    {
        // Arrange
        _repositorio.ActualizarAsync(Arg.Any<Contacto>()).Returns(true);
        var vm = CrearViewModel();
        vm.RecibirParametros(Parametros(new Contacto { Id = 7, Nombre = "Ana", Telefono = "342-5123456", Email = "ana@mail.com" }));
        vm.Telefono = "342-5999999";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        await _repositorio.Received(1).ActualizarAsync(Arg.Is<Contacto>(c => c.Id == 7 && c.Telefono == "342-5999999"));
        await _repositorio.DidNotReceive().InsertarAsync(Arg.Any<Contacto>());
        await _navegacion.Received(1).VolverAsync();
    }

    [Fact]
    public async Task Guardar_SiElRepositorioDevuelveFalse_AvisaYNoCierraElModal()
    {
        // Arrange
        _repositorio.InsertarAsync(Arg.Any<Contacto>()).Returns(false);
        var vm = CrearViewModel();
        vm.Nombre = "Ana";
        vm.Telefono = "342-5123456";
        vm.Email = "ana@mail.com";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
        await _navegacion.DidNotReceive().VolverAsync();
    }

    [Fact]
    public async Task Guardar_SiElRepositorioLanzaUnaExcepcion_AvisaYNoCierraElModal()
    {
        // Arrange
        _repositorio.InsertarAsync(Arg.Any<Contacto>()).ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();
        vm.Nombre = "Ana";
        vm.Telefono = "342-5123456";
        vm.Email = "ana@mail.com";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
        await _navegacion.DidNotReceive().VolverAsync();
    }

    [Fact]
    public async Task Guardar_ConDatosValidos_LimpiaLosErroresDeUnIntentoAnterior()
    {
        // Arrange: un primer intento con el nombre vacío deja un error visible.
        _repositorio.InsertarAsync(Arg.Any<Contacto>()).Returns(true);
        var vm = CrearViewModel();
        vm.Nombre = "";
        vm.Telefono = "342-5123456";
        vm.Email = "ana@mail.com";
        await vm.GuardarCommand.ExecuteAsync(null);
        Assert.NotEqual(string.Empty, vm.ErrorNombre);
        vm.Nombre = "Ana";

        // Act
        await vm.GuardarCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(string.Empty, vm.ErrorNombre);
    }

    // ------------------------------ Cancelar ------------------------------
    [Fact]
    public async Task Cancelar_VuelveSinGuardarNada()
    {
        // Arrange
        var vm = CrearViewModel();
        vm.Nombre = "Ana";

        // Act
        await vm.CancelarCommand.ExecuteAsync(null);

        // Assert
        await _navegacion.Received(1).VolverAsync();
        await _repositorio.DidNotReceive().InsertarAsync(Arg.Any<Contacto>());
        await _repositorio.DidNotReceive().ActualizarAsync(Arg.Any<Contacto>());
    }
}
