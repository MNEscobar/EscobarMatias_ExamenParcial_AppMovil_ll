namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Abstrae la consulta de conectividad (en MAUI, la clase estática Connectivity) para que el
    // servicio de API no dependa de la plataforma y se pueda probar simulando "con" o "sin" internet.
    public interface IConectividadService
    {
        // true si el dispositivo tiene acceso a internet.
        bool HayInternet { get; }
    }
}