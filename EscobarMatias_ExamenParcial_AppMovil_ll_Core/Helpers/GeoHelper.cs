using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Helpers
{
    // ==============================================================================================
    // Cálculos geográficos puros (sin dependencias de MAUI, de la red ni de ningún servicio),
    // por eso son estáticos y se pueden probar con valores conocidos.
    //
    // Fórmula de Haversine: distancia "en línea recta sobre la superficie" (círculo máximo) entre
    // dos puntos de una esfera. Es una aproximación muy buena (la Tierra no es una esfera perfecta,
    // el error típico es menor al 0,5 %) y alcanza de sobra para mostrar "estás a X km de ...".
    // ==============================================================================================
    public static class GeoHelper
    {
        // Radio medio de la Tierra en kilómetros.
        public const double RadioTierraKm = 6371.0088;

        // Distancia en km entre dos ubicaciones.
        public static double CalcularDistanciaKm(Ubicacion origen, Ubicacion destino)
        {
            ArgumentNullException.ThrowIfNull(origen);
            ArgumentNullException.ThrowIfNull(destino);

            return CalcularDistanciaKm(origen.Latitud, origen.Longitud, destino.Latitud, destino.Longitud);
        }

        // Distancia en km entre dos pares de coordenadas (grados decimales).
        // Lanza ArgumentOutOfRangeException si alguna coordenada no es un número o está fuera de rango.
        public static double CalcularDistanciaKm(double latitud1, double longitud1, double latitud2, double longitud2)
        {
            ValidarCoordenada(latitud1, Ubicacion.LatitudMaxima, nameof(latitud1));
            ValidarCoordenada(longitud1, Ubicacion.LongitudMaxima, nameof(longitud1));
            ValidarCoordenada(latitud2, Ubicacion.LatitudMaxima, nameof(latitud2));
            ValidarCoordenada(longitud2, Ubicacion.LongitudMaxima, nameof(longitud2));

            double fi1 = GradosARadianes(latitud1);
            double fi2 = GradosARadianes(latitud2);
            double deltaFi = GradosARadianes(latitud2 - latitud1);
            double deltaLambda = GradosARadianes(longitud2 - longitud1);

            // a = sen²(Δφ/2) + cos φ1 · cos φ2 · sen²(Δλ/2)
            double a = Math.Pow(Math.Sin(deltaFi / 2), 2)
                     + Math.Cos(fi1) * Math.Cos(fi2) * Math.Pow(Math.Sin(deltaLambda / 2), 2);

            // Por errores de redondeo "a" podría pasar apenas de 1 (en puntos antípodas) y Asin devolvería NaN.
            a = Math.Min(1.0, a);

            // d = 2 · R · asen(√a)
            return 2 * RadioTierraKm * Math.Asin(Math.Sqrt(a));
        }

        private static double GradosARadianes(double grados) => grados * Math.PI / 180.0;

        private static void ValidarCoordenada(double valor, double maximoAbsoluto, string nombreParametro)
        {
            if (!double.IsFinite(valor) || Math.Abs(valor) > maximoAbsoluto)
                throw new ArgumentOutOfRangeException(
                    nombreParametro, valor, $"El valor debe ser un número entre -{maximoAbsoluto} y {maximoAbsoluto}.");
        }
    }
}