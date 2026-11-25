namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Abstrae el GPS (en MAUI, Geolocation + Permissions) para que los ViewModels no dependan de la
    // plataforma y se puedan probar simulando cada situación (éxito, sin permiso, GPS apagado...).
    // La implementación real está en la app MAUI (UbicacionService) y se registra en MauiProgram.cs.
    public interface IUbicacionService
    {
        // Verifica/solicita el permiso de ubicación y obtiene la ubicación actual.
        // Nunca lanza excepciones: todo problema se informa en el EstadoUbicacion del resultado.
        Task<ResultadoUbicacion> ObtenerUbicacionActualAsync(CancellationToken cancellationToken = default);
    }
}