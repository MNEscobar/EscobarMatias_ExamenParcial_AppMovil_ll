using NSubstitute.ExceptionExtensions;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.ViewModels;

// ViewModel probado "en aislamiento": sus tres dependencias son mocks de NSubstitute,
// así que no hay base de datos, ni pantallas, ni Shell. Patrón AAA en todas las pruebas.
public class ContactosViewModelTests
{
    private readonly IContactoRepository _repositorio = Substitute.For<IContactoRepository>();
    private readonly IDialogService _dialogos = Substitute.For<IDialogService>();
    private readonly INavigationService _navegacion = Substitute.For<INavigationService>();

    private ContactosViewModel CrearViewModel() => new(_repositorio, _dialogos, _navegacion);

    private static List<Contacto> DosContactos() =>
    [
        new() { Id = 1, Nombre = "Ana", Telefono = "342-5123456", Email = "ana@mail.com" },
        new() { Id = 2, Nombre = "Juan", Telefono = "342-5234567", Email = "juan@mail.com" }
    ];

    // ------------------------------ Cargar ------------------------------
    [Fact]
    public async Task CargarContactos_LlenaLaColeccionConLosDelRepositorio()
    {
        // Arrange
        _repositorio.ObtenerTodosAsync().Returns(DosContactos());
        var vm = CrearViewModel();

        // Act
        await vm.CargarContactosCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, vm.Contactos.Count);
        Assert.Equal("Ana", vm.Contactos[0].Nombre);
        Assert.False(vm.EstaCargando);
    }

    [Fact]
    public async Task CargarContactos_ReemplazaLoQueHabiaEnLaColeccion()
    {
        // Arrange: una primera carga con dos contactos...
        var vm = CrearViewModel();
        _repositorio.ObtenerTodosAsync().Returns(DosContactos());
        await vm.CargarContactosCommand.ExecuteAsync(null);
        // ...y el repositorio pasa a devolver uno solo.
        _repositorio.ObtenerTodosAsync().Returns(new List<Contacto> { new() { Id = 3, Nombre = "Lucía" } });

        // Act
        await vm.CargarContactosCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(vm.Contactos);
        Assert.Equal("Lucía", vm.Contactos[0].Nombre);
    }

    [Fact]
    public async Task CargarContactos_SiElRepositorioLanza_AvisaAlUsuarioYNoQuedaCargando()
    {
        // Arrange
        _repositorio.ObtenerTodosAsync().ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.CargarContactosCommand.ExecuteAsync(null);

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
        Assert.False(vm.EstaCargando);
    }

    // ------------------------------ Buscar ------------------------------
    [Fact]
    public async Task Buscar_ConsultaAlRepositorioConElTextoDeBusqueda()
    {
        // Arrange
        _repositorio.BuscarPorNombreAsync("ana").Returns(new List<Contacto> { new() { Id = 1, Nombre = "Ana" } });
        var vm = CrearViewModel();
        vm.TextoBusqueda = "ana";

        // Act
        await vm.BuscarCommand.ExecuteAsync(null);

        // Assert
        await _repositorio.Received(1).BuscarPorNombreAsync("ana");
        Assert.Single(vm.Contactos);
    }

    [Fact]
    public async Task Buscar_SiElRepositorioLanza_AvisaAlUsuarioYNoQuedaCargando()
    {
        // Arrange
        _repositorio.BuscarPorNombreAsync(Arg.Any<string>()).ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();
        vm.TextoBusqueda = "ana";

        // Act
        await vm.BuscarCommand.ExecuteAsync(null);

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
        Assert.False(vm.EstaCargando);
    }

    // ----------------------------- Eliminar -----------------------------
    [Fact]
    public async Task Eliminar_ConfirmandoElimina_YRecargaLaListaDesdeElRepositorio()
    {
        // Arrange
        var ana = new Contacto { Id = 1, Nombre = "Ana" };
        _dialogos.ConfirmarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _repositorio.EliminarAsync(1).Returns(true);
        _repositorio.ObtenerTodosAsync().Returns(new List<Contacto> { new() { Id = 2, Nombre = "Juan" } });
        var vm = CrearViewModel();

        // Act
        await vm.EliminarContactoCommand.ExecuteAsync(ana);

        // Assert
        await _repositorio.Received(1).EliminarAsync(1);
        Assert.Single(vm.Contactos);
        Assert.Equal("Juan", vm.Contactos[0].Nombre);
    }

    [Fact]
    public async Task Eliminar_SiElUsuarioCancela_NoEliminaNada()
    {
        // Arrange
        _dialogos.ConfirmarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        var vm = CrearViewModel();

        // Act
        await vm.EliminarContactoCommand.ExecuteAsync(new Contacto { Id = 1, Nombre = "Ana" });

        // Assert
        await _repositorio.DidNotReceive().EliminarAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Eliminar_SiElRepositorioDevuelveFalse_AvisaAlUsuario()
    {
        // Arrange
        _dialogos.ConfirmarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _repositorio.EliminarAsync(1).Returns(false);
        var vm = CrearViewModel();

        // Act
        await vm.EliminarContactoCommand.ExecuteAsync(new Contacto { Id = 1, Nombre = "Ana" });

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Aviso"), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Eliminar_SiElRepositorioLanza_AvisaAlUsuario()
    {
        // Arrange
        _dialogos.ConfirmarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _repositorio.EliminarAsync(Arg.Any<int>()).ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.EliminarContactoCommand.ExecuteAsync(new Contacto { Id = 1, Nombre = "Ana" });

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
    }

    // ------------------------- Navegación al modal -------------------------
    [Fact]
    public async Task AgregarContacto_NavegaAlModalConUnContactoNuevo()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        await vm.AgregarContactoCommand.ExecuteAsync(null);

        // Assert
        await _navegacion.Received(1).NavegarAAsync(
            RutasNavegacion.ContactoModal,
            Arg.Is<IDictionary<string, object>?>(p =>
                p != null
                && p.ContainsKey(RutasNavegacion.ParametroContacto)
                && ((Contacto)p[RutasNavegacion.ParametroContacto]).Id == 0));
    }

    [Fact]
    public async Task EditarContacto_NavegaAlModalEnviandoElContactoSeleccionado()
    {
        // Arrange
        var ana = new Contacto { Id = 1, Nombre = "Ana" };
        var vm = CrearViewModel();

        // Act
        await vm.EditarContactoCommand.ExecuteAsync(ana);

        // Assert
        await _navegacion.Received(1).NavegarAAsync(
            RutasNavegacion.ContactoModal,
            Arg.Is<IDictionary<string, object>?>(p =>
                p != null && ReferenceEquals(p[RutasNavegacion.ParametroContacto], ana)));
    }

    [Fact]
    public async Task EditarContacto_ConNull_NoNavega()
    {
        // Arrange
        var vm = CrearViewModel();

        // Act
        await vm.EditarContactoCommand.ExecuteAsync(null);

        // Assert
        await _navegacion.DidNotReceive().NavegarAAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>());
    }

    [Fact]
    public async Task AgregarContacto_SiLaNavegacionFalla_AvisaAlUsuario()
    {
        // Arrange
        _navegacion.NavegarAAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>?>())
                   .ThrowsAsync(new InvalidOperationException("falla de prueba"));
        var vm = CrearViewModel();

        // Act
        await vm.AgregarContactoCommand.ExecuteAsync(null);

        // Assert
        await _dialogos.Received(1).MostrarAlertaAsync(Arg.Is("Error"), Arg.Any<string>(), Arg.Any<string>());
    }
}
