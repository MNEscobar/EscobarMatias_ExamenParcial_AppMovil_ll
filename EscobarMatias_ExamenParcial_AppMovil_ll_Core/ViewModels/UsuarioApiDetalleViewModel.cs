using CommunityToolkit.Mvvm.ComponentModel;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models.Api;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.ViewModels
{
    // ========================================================================================
    // ViewModel de la pantalla de detalle de un usuario de la API.
    // Implementa IRecibeParametros para recibir el UsuarioApi que envía UsuariosViewModel
    // (mismo mecanismo que ya usa ContactoDetalleViewModel).
    // ========================================================================================
    public partial class UsuarioApiDetalleViewModel : ObservableObject, IRecibeParametros
    {
        [ObservableProperty]
        private UsuarioApi? usuario;

        // La página llama a este método al navegar, pasando el diccionario de parámetros.
        public void RecibirParametros(IDictionary<string, object> parametros)
        {
            if (parametros.TryGetValue(RutasNavegacion.ParametroUsuario, out var valor) && valor is UsuarioApi usuarioRecibido)
                Usuario = usuarioRecibido;
        }
    }
}