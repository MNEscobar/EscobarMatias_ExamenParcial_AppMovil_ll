namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models
{
    // ==============================================================================================
    // Un punto geográfico en grados decimales (latitud y longitud, sistema WGS84).
    // Es un tipo de Core: no depende de MAUI. La clase Location de MAUI se convierte a este tipo
    // en UbicacionService, así el resto de la app (y los tests) nunca ven tipos de plataforma.
    // ==============================================================================================
    public sealed record Ubicacion(double Latitud, double Longitud)
    {
        public const double LatitudMaxima = 90;
        public const double LongitudMaxima = 180;

        // true si ambos valores son números finitos y están dentro del rango geográfico válido
        // (latitud entre -90 y 90, longitud entre -180 y 180).
        public bool EsValida =>
            double.IsFinite(Latitud) && double.IsFinite(Longitud)
            && Math.Abs(Latitud) <= LatitudMaxima
            && Math.Abs(Longitud) <= LongitudMaxima;
    }
}