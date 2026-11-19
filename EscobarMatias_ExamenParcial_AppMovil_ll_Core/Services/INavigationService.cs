namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Abstrae la navegación para que los ViewModels no dependan de Shell (una clase de la interfaz
    // gráfica). Se registra en MauiProgram.cs y llega por constructor, igual que IDialogService.
    public interface INavigationService
    {
        // Navega a una ruta registrada, opcionalmente enviando parámetros a la pantalla de destino.
        Task NavegarAAsync(string ruta, IDictionary<string, object>? parametros = null);

        // Vuelve a la pantalla anterior (cierra un modal o desapila la página actual).
        Task VolverAsync();
    }
}