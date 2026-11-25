using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;

namespace EscobarMatias_ExamenParcial_AppMovil_ll.Services
{
    // ==============================================================================================
    // Implementación de IVibracionService con Vibration.Default de .NET MAUI.
    // Cumple el contrato de la interfaz: NUNCA lanza excepciones (si el dispositivo no vibra, no hace nada).
    //
    // Android necesita el permiso VIBRATE en Platforms/Android/AndroidManifest.xml.
    // iOS no necesita configuración, pero ignora la duración y usa un valor fijo.
    // ==============================================================================================
    public class VibracionService : IVibracionService
    {
        public void Vibrar(TimeSpan duracion)
        {
            try
            {
                // Hay dispositivos (emuladores, equipos de escritorio) sin motor de vibración.
                if (!Vibration.Default.IsSupported)
                    return;

                Vibration.Default.Vibrate(duracion);
            }
            catch (FeatureNotSupportedException ex)
            {
                Debug.WriteLine($"[VibracionService] Vibración no soportada: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VibracionService] Error al vibrar: {ex}");
            }
        }
    }
}