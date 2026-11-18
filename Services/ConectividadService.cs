using Microsoft.Maui.Networking;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.Services
{
    // Implementación de IConectividadService apoyada en Connectivity de .NET MAUI.
    public class ConectividadService : IConectividadService
    {
        public bool HayInternet => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}