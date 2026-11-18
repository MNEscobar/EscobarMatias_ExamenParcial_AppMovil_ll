using CommunityToolkit.Mvvm.ComponentModel;
using EscobarMatias_ActividadPractica_AppMovil_ll.Models.Api;
using EscobarMatias_ActividadPractica_AppMovil_ll.Services;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels
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