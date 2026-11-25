using NSubstitute.ExceptionExtensions;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.Helpers;

// Pruebas del cálculo de distancia (Haversine) con coordenadas conocidas.
// Es una función pura: no necesita mocks. Patrón AAA en todas las pruebas.
public class GeoHelperTests
{
    private const double PiPorRadio = Math.PI * GeoHelper.RadioTierraKm;   // media circunferencia ≈ 20 015 km

    // ------------------------------ Casos básicos ------------------------------
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-34.6037, -58.3816)]
    [InlineData(90, 0)]
    [InlineData(-90, 180)]
    public void MismoPunto_DevuelveCeroKm(double latitud, double longitud)
    {
        // Arrange: el punto llega por [InlineData].

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(latitud, longitud, latitud, longitud);

        // Assert
        Assert.Equal(0.0, distancia);
    }

    // Distancias entre ciudades conocidas (valores de referencia redondeados) con una tolerancia razonable.
    [Theory]
    [InlineData(-34.6037, -58.3816, -34.9011, -56.1645, 205.2, 2.0)]    // Buenos Aires - Montevideo
    [InlineData(51.5074, -0.1278, 48.8566, 2.3522, 343.6, 2.0)]         // Londres - París
    [InlineData(40.7128, -74.0060, 51.5074, -0.1278, 5570.0, 20.0)]     // Nueva York - Londres
    [InlineData(-34.6037, -58.3816, -31.4201, -64.1888, 646.7, 5.0)]    // Buenos Aires - Córdoba
    public void CiudadesConocidas_DevuelvenLaDistanciaEsperada(
        double lat1, double lon1, double lat2, double lon2, double esperadoKm, double toleranciaKm)
    {
        // Arrange: las coordenadas y el valor esperado llegan por [InlineData].

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(lat1, lon1, lat2, lon2);

        // Assert
        Assert.InRange(distancia, esperadoKm - toleranciaKm, esperadoKm + toleranciaKm);
    }

    // ------------------------------ Casos de borde ------------------------------
    [Fact]
    public void UnGradoDeLongitudEnElEcuador_Mide111Km()
    {
        // Arrange
        double unGradoEnKm = GeoHelper.RadioTierraKm * Math.PI / 180.0;

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(0, 0, 0, 1);

        // Assert
        Assert.InRange(distancia, unGradoEnKm - 0.001, unGradoEnKm + 0.001);
        Assert.InRange(distancia, 111.0, 111.4);
    }

    [Fact]
    public void PuntosAntipodas_DistanMediaCircunferenciaDeLaTierra()
    {
        // Arrange: el punto (0, 0) y el punto opuesto del planeta (0, 180).

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(0, 0, 0, 180);

        // Assert
        Assert.InRange(distancia, PiPorRadio - 0.01, PiPorRadio + 0.01);
    }

    [Fact]
    public void DeUnPoloAlOtro_EsMediaCircunferencia()
    {
        // Arrange: Polo Norte y Polo Sur.

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(90, 0, -90, 0);

        // Assert
        Assert.InRange(distancia, PiPorRadio - 0.01, PiPorRadio + 0.01);
    }

    [Fact]
    public void CruzandoElAntimeridiano_TomaElCaminoCorto()
    {
        // Arrange: 179°E y 179°O están a solo 2 grados de distancia, no a 358.
        double dosGradosEnKm = 2 * GeoHelper.RadioTierraKm * Math.PI / 180.0;

        // Act
        double distancia = GeoHelper.CalcularDistanciaKm(0, 179, 0, -179);

        // Assert
        Assert.InRange(distancia, dosGradosEnKm - 0.001, dosGradosEnKm + 0.001);
    }

    [Fact]
    public void LosLimitesValidosDeCoordenadas_SeAceptan()
    {
        // Arrange + Act
        double distancia = GeoHelper.CalcularDistanciaKm(90, 180, -90, -180);

        // Assert: no lanza y devuelve un número finito, no negativo.
        Assert.True(double.IsFinite(distancia));
        Assert.True(distancia >= 0);
    }

    // ------------------------------ Propiedades ------------------------------
    [Fact]
    public void LaDistanciaEsSimetrica()
    {
        // Arrange
        var buenosAires = new Ubicacion(-34.6037, -58.3816);
        var londres = new Ubicacion(51.5074, -0.1278);

        // Act
        double ida = GeoHelper.CalcularDistanciaKm(buenosAires, londres);
        double vuelta = GeoHelper.CalcularDistanciaKm(londres, buenosAires);

        // Assert
        Assert.Equal(ida, vuelta, precision: 9);
    }

    [Fact]
    public void LaSobrecargaConUbicacion_DaElMismoResultadoQueConNumeros()
    {
        // Arrange
        var origen = new Ubicacion(-34.6037, -58.3816);
        var destino = new Ubicacion(-34.9011, -56.1645);

        // Act
        double conObjetos = GeoHelper.CalcularDistanciaKm(origen, destino);
        double conNumeros = GeoHelper.CalcularDistanciaKm(-34.6037, -58.3816, -34.9011, -56.1645);

        // Assert
        Assert.Equal(conNumeros, conObjetos);
    }

    // ------------------------------ Entradas inválidas ------------------------------
    [Theory]
    [InlineData(91, 0, 0, 0)]
    [InlineData(-91, 0, 0, 0)]
    [InlineData(0, 181, 0, 0)]
    [InlineData(0, -181, 0, 0)]
    [InlineData(0, 0, 90.0001, 0)]
    [InlineData(0, 0, 0, 180.0001)]
    public void CoordenadasFueraDeRango_LanzanArgumentOutOfRangeException(double lat1, double lon1, double lat2, double lon2)
    {
        // Arrange: las coordenadas inválidas llegan por [InlineData].

        // Test 1: CoordenadasFueraDeRango_LanzanArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoHelper.CalcularDistanciaKm(lat1, lon1, lat2, lon2));
    }

    [Fact]
    public void ConUnValorNoNumerico_LanzaArgumentOutOfRangeException()
    {
        // Arrange
        double noNumerico = double.NaN;

        // Test 2: ConUnValorNoNumerico_LanzaArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoHelper.CalcularDistanciaKm(noNumerico, 0, 0, 0));
    }

    [Fact]
    public void ConUnaUbicacionNula_LanzaArgumentNullException()
    {
        // Arrange
        var destino = new Ubicacion(0, 0);

        // Test 3: ConUnaUbicacionNula_LanzaArgumentNullException
        Action accion = () => GeoHelper.CalcularDistanciaKm(null!, destino);

    }
}