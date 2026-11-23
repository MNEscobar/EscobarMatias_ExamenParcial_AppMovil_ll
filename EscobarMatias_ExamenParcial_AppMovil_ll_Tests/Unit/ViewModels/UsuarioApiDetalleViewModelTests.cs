namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.ViewModels;

public class UsuarioApiDetalleViewModelTests
{
    [Fact]
    public void RecibirParametros_ConUnUsuario_LoExponeParaLaPantalla()
    {
        // Arrange
        var vm = new UsuarioApiDetalleViewModel();
        var usuario = new UsuarioApi { Id = 3, Nombre = "Clementine Bauch" };
        var parametros = new Dictionary<string, object> { [RutasNavegacion.ParametroUsuario] = usuario };

        // Act
        vm.RecibirParametros(parametros);

        // Assert
        Assert.Same(usuario, vm.Usuario);
    }

    [Fact]
    public void RecibirParametros_ConUnObjetoDeOtroTipo_NoCambiaElUsuario()
    {
        // Arrange
        var vm = new UsuarioApiDetalleViewModel();
        var parametros = new Dictionary<string, object> { [RutasNavegacion.ParametroUsuario] = "no soy un usuario" };

        // Act
        vm.RecibirParametros(parametros);

        // Assert
        Assert.Null(vm.Usuario);
    }

    [Fact]
    public void RecibirParametros_SinLaClaveEsperada_NoCambiaElUsuario()
    {
        // Arrange
        var vm = new UsuarioApiDetalleViewModel();
        var parametros = new Dictionary<string, object> { ["OtraClave"] = new UsuarioApi() };

        // Act
        vm.RecibirParametros(parametros);

        // Assert
        Assert.Null(vm.Usuario);
    }

    [Fact]
    public void Usuario_AntesDeRecibirParametros_EsNulo()
    {
        // Arrange + Act
        var vm = new UsuarioApiDetalleViewModel();

        // Assert
        Assert.Null(vm.Usuario);
    }
}
