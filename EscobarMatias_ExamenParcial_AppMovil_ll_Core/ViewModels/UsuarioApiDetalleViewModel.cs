using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Helpers;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models.Api;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.ViewModels
{
    // ========================================================================================
    // ViewModel de la pantalla de detalle de un usuario de la API.
    // - Recibe el UsuarioApi que envía UsuariosViewModel (IRecibeParametros).
    // - Permite calcular la distancia entre el dispositivo (GPS) y las coordenadas del usuario.
    //   El GPS se pide a IUbicacionService y la fórmula vive en GeoHelper: este ViewModel solo
    //   coordina y traduce cada resultado a un mensaje claro, sin conocer MAUI.
    // ========================================================================================
    public partial class UsuarioApiDetalleViewModel : ObservableObject, IRecibeParametros
    {
        private readonly IUbicacionService _ubicacionService;

        [ObservableProperty]
        private UsuarioApi? usuario;

        // true mientras se espera la respuesta del GPS (la vista muestra un ActivityIndicator
        // y el botón se deshabilita).
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CalcularDistanciaCommand))]
        private bool estaBuscandoUbicacion;

        // Texto con el resultado (o el error) del cálculo de distancia.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HayMensajeDistancia))]
        private string mensajeDistancia = string.Empty;

        // true si el último intento terminó mal: la vista pinta el mensaje de rojo.
        [ObservableProperty]
        private bool hayErrorDistancia;

        // Distancia calculada en kilómetros (null si todavía no se calculó o si falló).
        [ObservableProperty]
        private double? distanciaKm;

        // Permite que la vista muestre el Label solo cuando hay algo que decir.
        public bool HayMensajeDistancia => !string.IsNullOrEmpty(MensajeDistancia);

        public UsuarioApiDetalleViewModel(IUbicacionService ubicacionService)
        {
            _ubicacionService = ubicacionService;
        }

        // La página llama a este método al navegar, pasando el diccionario de parámetros.
        public void RecibirParametros(IDictionary<string, object> parametros)
        {
            if (parametros.TryGetValue(RutasNavegacion.ParametroUsuario, out var valor) && valor is UsuarioApi usuarioRecibido)
            {
                Usuario = usuarioRecibido;
                LimpiarResultadoDeDistancia();
            }
        }

        [RelayCommand(CanExecute = nameof(PuedeCalcularDistancia))]
        private async Task CalcularDistanciaAsync()
        {
            if (Usuario is null)
                return;

            LimpiarResultadoDeDistancia();

            // 1) ¿El usuario tiene coordenadas válidas? Si no, no tiene sentido pedir el GPS.
            Ubicacion? destino = Usuario.Direccion?.Geo?.ObtenerUbicacion();
            if (destino is null)
            {
                PublicarError("Este usuario no tiene coordenadas válidas.");
                return;
            }

            EstaBuscandoUbicacion = true;
            MensajeDistancia = "Obteniendo tu ubicación...";

            try
            {
                // 2) Pedir la ubicación actual (el servicio se ocupa del permiso y del GPS).
                var resultado = await _ubicacionService.ObtenerUbicacionActualAsync();

                if (resultado.EsExitoso && resultado.Datos is not null)
                {
                    // 3) Calcular la distancia con la fórmula de Haversine.
                    double km = GeoHelper.CalcularDistanciaKm(resultado.Datos, destino);
                    DistanciaKm = km;
                    MensajeDistancia = $"Estás a {FormatearDistancia(km)} de {Usuario.Nombre}.";
                }
                else
                {
                    PublicarError(ObtenerMensajeDeError(resultado.Estado));
                }
            }
            catch (Exception ex)
            {
                // Red de seguridad: el servicio ya captura sus errores, pero esto evita
                // que una excepción no prevista cierre la aplicación.
                System.Diagnostics.Debug.WriteLine($"[UsuarioApiDetalleViewModel] {ex}");
                PublicarError("Ocurrió un error inesperado al obtener tu ubicación.");
            }
            finally
            {
                EstaBuscandoUbicacion = false;
            }
        }

        private bool PuedeCalcularDistancia() => !EstaBuscandoUbicacion;

        private void LimpiarResultadoDeDistancia()
        {
            DistanciaKm = null;
            HayErrorDistancia = false;
            MensajeDistancia = string.Empty;
        }

        private void PublicarError(string mensaje)
        {
            DistanciaKm = null;
            HayErrorDistancia = true;
            MensajeDistancia = mensaje;
        }

        // Traduce cada estado del GPS a un mensaje claro para el usuario final.
        private static string ObtenerMensajeDeError(EstadoUbicacion estado) => estado switch
        {
            EstadoUbicacion.PermisoDenegado =>
                "No diste permiso de ubicación. Habilitalo en los ajustes de la app para calcular la distancia.",
            EstadoUbicacion.GpsDesactivado =>
                "El GPS está desactivado. Activá la ubicación del dispositivo e intentá de nuevo.",
            EstadoUbicacion.NoSoportado =>
                "Este dispositivo no permite obtener la ubicación.",
            _ => "No se pudo obtener tu ubicación. Intentá de nuevo."
        };

        // Menos de 1 km se muestra así; el resto, redondeado y con el formato numérico del dispositivo.
        private static string FormatearDistancia(double km) =>
            km < 1 ? "menos de 1 km" : $"{km.ToString("N0", CultureInfo.CurrentCulture)} km";
    }
}