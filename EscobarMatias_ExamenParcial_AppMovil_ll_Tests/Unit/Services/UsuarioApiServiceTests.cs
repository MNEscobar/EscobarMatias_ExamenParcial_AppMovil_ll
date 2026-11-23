using System.Net;
using EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Fakes;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.Services;

// ==============================================================================================
// Pruebas del servicio de API SIN usar internet:
//  - el HttpClient usa un "servidor falso" (FakeHttpMessageHandler) que responde lo que cada prueba pida;
//  - la conectividad es un mock (NSubstitute) de IConectividadService.
// Así se pueden provocar a voluntad casos que con la API real casi nunca ocurren (400, 500, timeout...).
// ==============================================================================================
public class UsuarioApiServiceTests
{
    private const string JsonDosUsuarios = """
        [
          {
            "id": 1, "name": "Leanne Graham", "username": "Bret", "email": "sincere@april.biz",
            "phone": "1-770-736-8031", "website": "hildegard.org",
            "address": { "street": "Kulas Light", "city": "Gwenborough" },
            "company": { "name": "Romaguera-Crona", "catchPhrase": "Multi-layered client-server neural-net" }
          },
          {
            "id": 2, "name": "Ervin Howell", "username": "Antonette", "email": "shanna@melissa.tv",
            "phone": "010-692-6593", "website": "anastasia.net",
            "address": { "street": "Victor Plains", "city": "Wisokyburgh" },
            "company": { "name": "Deckow-Crist", "catchPhrase": "Proactive didactic contingency" }
          }
        ]
        """;

    // Arma el servicio real con un HttpClient falso y una conectividad simulada.
    private static UsuarioApiService CrearServicio(FakeHttpMessageHandler handler, bool hayInternet = true)
    {
        var conectividad = Substitute.For<IConectividadService>();
        conectividad.HayInternet.Returns(hayInternet);

        var cliente = new HttpClient(handler) { BaseAddress = new Uri("https://api.prueba/") };

        return new UsuarioApiService(cliente, conectividad);
    }

    // ------------------------------ HTTP 200 ------------------------------
    [Fact]
    public async Task Respuesta200ConUsuarios_DevuelveLosDatosDeserializados()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, JsonDosUsuarios);
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.True(resultado.EsExitoso);
        Assert.Equal(TipoErrorApi.Ninguno, resultado.Error);
        Assert.Equal(200, resultado.CodigoHttp);
        Assert.NotNull(resultado.Datos);
        Assert.Equal(2, resultado.Datos!.Count);

        // El mapeo JSON (inglés) -> DTO (español), incluidos los objetos anidados.
        var primero = resultado.Datos[0];
        Assert.Equal(1, primero.Id);
        Assert.Equal("Leanne Graham", primero.Nombre);
        Assert.Equal("Bret", primero.NombreUsuario);
        Assert.Equal("sincere@april.biz", primero.Email);
        Assert.Equal("Gwenborough", primero.Direccion.Ciudad);
        Assert.Equal("Romaguera-Crona", primero.Empresa.Nombre);
    }

    [Fact]
    public async Task Respuesta200_PideLaRutaUsersRelativaALaUrlBase()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, JsonDosUsuarios);
        var servicio = CrearServicio(handler);

        // Act
        await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(1, handler.Llamadas);
        Assert.Equal("https://api.prueba/users", handler.UltimaSolicitud!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, handler.UltimaSolicitud.Method);
    }

    // --------------------------- Sin conexión ---------------------------
    [Fact]
    public async Task SinInternet_DevuelveSinConexion_YNoLlegaALaApi()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, JsonDosUsuarios);
        var servicio = CrearServicio(handler, hayInternet: false);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.False(resultado.EsExitoso);
        Assert.Equal(TipoErrorApi.SinConexion, resultado.Error);
        Assert.Null(resultado.Datos);
        Assert.Equal(0, handler.Llamadas);
    }

    [Fact]
    public async Task ErrorDeRed_DevuelveSinConexion()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Lanza(new HttpRequestException("No se pudo resolver el host"));
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.SinConexion, resultado.Error);
        Assert.Null(resultado.CodigoHttp);
    }

    // ----------------------------- Timeout ------------------------------
    [Fact]
    public async Task TimeoutDelCliente_DevuelveTiempoAgotado()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Lanza(new TaskCanceledException("Se agotó el tiempo de espera"));
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.TiempoAgotado, resultado.Error);
    }

    // -------------------------- Errores HTTP ---------------------------
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, TipoErrorApi.SolicitudInvalida)]          // 400
    [InlineData(HttpStatusCode.Unauthorized, TipoErrorApi.NoAutorizado)]             // 401
    [InlineData(HttpStatusCode.Forbidden, TipoErrorApi.NoAutorizado)]                // 403
    [InlineData(HttpStatusCode.NotFound, TipoErrorApi.NoEncontrado)]                 // 404
    [InlineData(HttpStatusCode.InternalServerError, TipoErrorApi.ErrorServidor)]     // 500
    [InlineData(HttpStatusCode.ServiceUnavailable, TipoErrorApi.ErrorServidor)]      // 503
    [InlineData((HttpStatusCode)418, TipoErrorApi.Desconocido)]                      // cualquier otro código
    public async Task CodigoHttpDeError_SeClasificaYConservaElCodigo(HttpStatusCode codigo, TipoErrorApi esperado)
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(codigo, "{}");
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.False(resultado.EsExitoso);
        Assert.Equal(esperado, resultado.Error);
        Assert.Equal((int)codigo, resultado.CodigoHttp);
        Assert.Null(resultado.Datos);
    }

    // ----------------------- Respuestas "raras" con 200 ----------------------
    [Fact]
    public async Task Respuesta200ConListaVacia_DevuelveSinDatos()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, "[]");
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.SinDatos, resultado.Error);
        Assert.Equal(200, resultado.CodigoHttp);
    }

    [Fact]
    public async Task Respuesta200ConJsonMalformado_DevuelveRespuestaInvalida()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, "esto no es un json");
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.RespuestaInvalida, resultado.Error);
    }

    [Fact]
    public async Task Respuesta200ConContenidoQueNoEsJson_DevuelveRespuestaInvalida()
    {
        // Arrange: por ejemplo, la página HTML de un portal cautivo en lugar de la API.
        var handler = FakeHttpMessageHandler.Responde(HttpStatusCode.OK, "<html><body>Iniciá sesión</body></html>", "text/html");
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.RespuestaInvalida, resultado.Error);
    }

    // ------------------------- Error inesperado -------------------------
    [Fact]
    public async Task ExcepcionInesperada_DevuelveDesconocido_SinLanzarla()
    {
        // Arrange
        var handler = FakeHttpMessageHandler.Lanza(new InvalidOperationException("algo no previsto"));
        var servicio = CrearServicio(handler);

        // Act
        var resultado = await servicio.ObtenerUsuariosAsync();

        // Assert
        Assert.Equal(TipoErrorApi.Desconocido, resultado.Error);
    }
}
