using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;

namespace EscobarMatias_ExamenParcial_AppMovil_ll.Services
{
    // ==============================================================================================
    // Implementación de IUbicacionService con las APIs de .NET MAUI:
    //   1) Permisos en tiempo de ejecución: Permissions.CheckStatusAsync / RequestAsync<LocationWhenInUse>.
    //   2) Lectura del GPS: Geolocation.Default.GetLocationAsync.
    // Todas las excepciones de plataforma se convierten en un EstadoUbicacion: esta clase nunca las deja escapar.
    //
    // Los permisos también deben declararse en los manifiestos:
    //   Android -> Platforms/Android/AndroidManifest.xml (ACCESS_COARSE_LOCATION y ACCESS_FINE_LOCATION)
    //   iOS     -> Platforms/iOS/Info.plist (NSLocationWhenInUseUsageDescription)
    // ==============================================================================================
    public class UbicacionService : IUbicacionService
    {
        // Tiempo máximo de espera de una lectura del GPS.
        private static readonly TimeSpan TiempoMaximoDeEspera = TimeSpan.FromSeconds(10);

        public async Task<ResultadoUbicacion> ObtenerUbicacionActualAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // 1) Permiso en tiempo de ejecución: se consulta y, si hace falta, se le pide al usuario.
                PermissionStatus permiso = await ObtenerPermisoDeUbicacionAsync();
                if (permiso != PermissionStatus.Granted)
                    return ResultadoUbicacion.Falla(EstadoUbicacion.PermisoDenegado);

                // 2) Lectura del GPS con precisión media (suficiente para calcular distancias entre ciudades).
                var solicitud = new GeolocationRequest(GeolocationAccuracy.Medium, TiempoMaximoDeEspera);
                Location? ubicacion = await Geolocation.Default.GetLocationAsync(solicitud, cancellationToken);

                // null = el dispositivo no pudo determinar la posición (sin señal, por ejemplo).
                if (ubicacion is null)
                    return ResultadoUbicacion.Falla(EstadoUbicacion.Error);

                return ResultadoUbicacion.Ok(new Ubicacion(ubicacion.Latitude, ubicacion.Longitude));
            }
            catch (FeatureNotSupportedException ex)
            {
                Debug.WriteLine($"[UbicacionService] Geolocalización no soportada: {ex.Message}");
                return ResultadoUbicacion.Falla(EstadoUbicacion.NoSoportado);
            }
            catch (FeatureNotEnabledException ex)
            {
                Debug.WriteLine($"[UbicacionService] GPS desactivado: {ex.Message}");
                return ResultadoUbicacion.Falla(EstadoUbicacion.GpsDesactivado);
            }
            catch (PermissionException ex)
            {
                Debug.WriteLine($"[UbicacionService] Permiso de ubicación denegado: {ex.Message}");
                return ResultadoUbicacion.Falla(EstadoUbicacion.PermisoDenegado);
            }
            catch (Exception ex)
            {
                // Incluye la cancelación o el tiempo agotado y cualquier falla no prevista de la plataforma.
                Debug.WriteLine($"[UbicacionService] Error al obtener la ubicación: {ex}");
                return ResultadoUbicacion.Falla(EstadoUbicacion.Error);
            }
        }

        // Consulta el permiso y, si todavía no fue concedido, muestra el diálogo del sistema.
        // MAUI exige pedir permisos desde el hilo principal, por eso se usa MainThread.
        private static Task<PermissionStatus> ObtenerPermisoDeUbicacionAsync()
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                PermissionStatus estado = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (estado != PermissionStatus.Granted)
                    estado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                return estado;
            });
        }
    }
}