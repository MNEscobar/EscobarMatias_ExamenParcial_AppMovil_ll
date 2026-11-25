using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Qué pasó al pedir la ubicación del dispositivo.
    public enum EstadoUbicacion
    {
        Exito,
        PermisoDenegado,   // el usuario no concedió el permiso de ubicación
        GpsDesactivado,    // el servicio de ubicación del dispositivo está apagado
        NoSoportado,       // el dispositivo/plataforma no ofrece geolocalización
        Error              // cualquier otro problema (sin señal, tiempo agotado, falla inesperada...)
    }

    // ==============================================================================================
    // Resultado de pedir la ubicación: o trae los datos (Datos) o trae el motivo del fallo (Estado).
    // Mismo patrón que ApiResult: el servicio nunca muestra mensajes ni deja escapar excepciones;
    // el ViewModel decide qué mensaje mostrar para cada estado.
    // ==============================================================================================
    public sealed record ResultadoUbicacion(Ubicacion? Datos, EstadoUbicacion Estado)
    {
        public bool EsExitoso => Estado == EstadoUbicacion.Exito;

        public static ResultadoUbicacion Ok(Ubicacion ubicacion)
        {
            ArgumentNullException.ThrowIfNull(ubicacion);
            return new ResultadoUbicacion(ubicacion, EstadoUbicacion.Exito);
        }

        public static ResultadoUbicacion Falla(EstadoUbicacion estado)
        {
            if (estado == EstadoUbicacion.Exito)
                throw new ArgumentException("Un fallo no puede tener el estado Exito.", nameof(estado));

            return new ResultadoUbicacion(null, estado);
        }
    }
}