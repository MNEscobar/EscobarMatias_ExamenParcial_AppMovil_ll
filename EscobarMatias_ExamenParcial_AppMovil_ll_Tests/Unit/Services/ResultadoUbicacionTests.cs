namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.Services;

public class ResultadoUbicacionTests
{
    [Fact]
    public void Ok_CreaUnResultadoExitosoConLosDatos()
    {
        // Arrange
        var ubicacion = new Ubicacion(-34.6037, -58.3816);

        // Act
        var resultado = ResultadoUbicacion.Ok(ubicacion);

        // Assert
        Assert.True(resultado.EsExitoso);
        Assert.Equal(EstadoUbicacion.Exito, resultado.Estado);
        Assert.Same(ubicacion, resultado.Datos);
    }

    [Fact]
    public void Ok_ConUbicacionNula_LanzaArgumentNullException()
    {
        // Arrange + Act
        var accion = () => ResultadoUbicacion.Ok(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(accion);
    }

    [Theory]
    [InlineData(EstadoUbicacion.PermisoDenegado)]
    [InlineData(EstadoUbicacion.GpsDesactivado)]
    [InlineData(EstadoUbicacion.NoSoportado)]
    [InlineData(EstadoUbicacion.Error)]
    public void Falla_CreaUnResultadoNoExitosoSinDatos(EstadoUbicacion estado)
    {
        // Arrange: el estado llega por [InlineData].

        // Act
        var resultado = ResultadoUbicacion.Falla(estado);

        // Assert
        Assert.False(resultado.EsExitoso);
        Assert.Equal(estado, resultado.Estado);
        Assert.Null(resultado.Datos);
    }

    [Fact]
    public void Falla_ConEstadoExito_LanzaArgumentException()
    {
        // Arrange + Act
        var accion = () => ResultadoUbicacion.Falla(EstadoUbicacion.Exito);

        // Assert
        Assert.Throws<ArgumentException>(accion);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(90, 180, true)]
    [InlineData(-90, -180, true)]
    [InlineData(90.1, 0, false)]
    [InlineData(0, 180.1, false)]
    [InlineData(double.NaN, 0, false)]
    [InlineData(0, double.PositiveInfinity, false)]
    public void Ubicacion_EsValida_SoloDentroDelRangoGeografico(double latitud, double longitud, bool esperado)
    {
        // Arrange
        var ubicacion = new Ubicacion(latitud, longitud);

        // Act
        bool resultado = ubicacion.EsValida;

        // Assert
        Assert.Equal(esperado, resultado);
    }
}