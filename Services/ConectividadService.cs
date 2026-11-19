using Microsoft.Maui.Networking;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;

namespace EscobarMatias_ExamenParcial_AppMovil_ll.Services
{
    // Implementación de IConectividadService apoyada en Connectivity de .NET MAUI.
    public class ConectividadService : IConectividadService
    {
        public bool HayInternet => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}