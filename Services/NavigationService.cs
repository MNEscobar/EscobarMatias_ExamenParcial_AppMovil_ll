namespace EscobarMatias_ActividadPractica_AppMovil_ll.Services
{
    // Implementación de INavigationService apoyada en Shell.
    public class NavigationService : INavigationService
    {
        public Task NavegarAAsync(string ruta, IDictionary<string, object>? parametros = null)
        {
            if (Shell.Current is null)
                return Task.CompletedTask;

            return parametros is null
                ? Shell.Current.GoToAsync(ruta)
                : Shell.Current.GoToAsync(ruta, parametros);
        }

        public Task VolverAsync()
        {
            if (Shell.Current is null)
                return Task.CompletedTask;

            // sintaxis de Shell para "volver a la página anterior".
            return Shell.Current.GoToAsync("..");
        }
    }
}