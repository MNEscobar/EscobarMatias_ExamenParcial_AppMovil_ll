using System.Text.Json;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.Models;

// Pruebas del DTO: que las coordenadas "geo" de JSONPlaceholder se deserialicen y se conviertan a Ubicacion.
public class UsuarioApiTests
{
    // Fragmento con la misma forma que devuelve https://jsonplaceholder.typicode.com/users
    // (las coordenadas llegan como TEXTO, no como número).
    private const string JsonConGeo = """
        {
          "id": 1,
          "name": "Leanne Graham",
          "username": "Bret",
          "email": "sincere@april.biz",
          "address": {
            "street": "Kulas Light",
            "suite": "Apt. 556",
            "city": "Gwenborough",
            "zipcode": "92998-3874",
            "geo": { "lat": "-37.3159", "lng": "81.1496" }
          }
        }
        """;

    private const string JsonSinGeo = """
        {
          "id": 2,
          "name": "Ervin Howell",
          "address": { "street": "Victor Plains", "city": "Wisokyburgh" }
        }
        """;

    [Fact]
    public void Deserializar_ConCoordenadasComoTexto_LeeLatitudYLongitud()
    {
        // Arrange + Act
        var usuario = JsonSerializer.Deserialize<UsuarioApi>(JsonConGeo);

        // Assert
        Assert.NotNull(usuario);
        Assert.Equal("-37.3159", usuario!.Direccion.Geo.Latitud);
        Assert.Equal("81.1496", usuario.Direccion.Geo.Longitud);
    }

    [Fact]
    public void ObtenerUbicacion_ConCoordenadasValidas_DevuelveLaUbicacion()
    {
        // Arrange
        var usuario = JsonSerializer.Deserialize<UsuarioApi>(JsonConGeo)!;

        // Act
        Ubicacion? ubicacion = usuario.Direccion.Geo.ObtenerUbicacion();

        // Assert
        Assert.NotNull(ubicacion);
        Assert.Equal(-37.3159, ubicacion!.Latitud);
        Assert.Equal(81.1496, ubicacion.Longitud);
    }

    [Fact]
    public void Deserializar_SinGeo_DejaCoordenadasVacias_YObtenerUbicacionDevuelveNull()
    {
        // Arrange
        var usuario = JsonSerializer.Deserialize<UsuarioApi>(JsonSinGeo)!;

        // Act
        Ubicacion? ubicacion = usuario.Direccion.Geo.ObtenerUbicacion();

        // Assert
        Assert.Equal(string.Empty, usuario.Direccion.Geo.Latitud);
        Assert.Null(ubicacion);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("abc", "10")]
    [InlineData("10", "abc")]
    [InlineData("91", "10")]          // latitud fuera de rango
    [InlineData("10", "181")]         // longitud fuera de rango
    [InlineData("-37,3159", "10")]    // coma decimal: no es el formato de la API
    [InlineData("NaN", "10")]
    public void ObtenerUbicacion_ConValoresInvalidos_DevuelveNull(string latitud, string longitud)
    {
        // Arrange
        var geo = new GeoApi { Latitud = latitud, Longitud = longitud };

        // Act
        Ubicacion? ubicacion = geo.ObtenerUbicacion();

        // Assert
        Assert.Null(ubicacion);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("90", "180")]
    [InlineData("-90", "-180")]
    [InlineData(" 12.5 ", " -7.25 ")]   // se toleran espacios alrededor
    public void ObtenerUbicacion_ConValoresValidosEnLosLimites_DevuelveUbicacion(string latitud, string longitud)
    {
        // Arrange
        var geo = new GeoApi { Latitud = latitud, Longitud = longitud };

        // Act
        Ubicacion? ubicacion = geo.ObtenerUbicacion();

        // Assert
        Assert.NotNull(ubicacion);
        Assert.True(ubicacion!.EsValida);
    }
}