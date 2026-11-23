using System.Net;
using System.Text;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Fakes;

// ==============================================================================================
// "Servidor falso": reemplaza al handler real de HttpClient para que cada prueba decida qué
// responde la API (o qué error ocurre) sin tocar la red. Es un fake escrito a mano.
// ==============================================================================================
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respuesta;

    // Cuántas veces el código bajo prueba llegó a "salir a la red".
    public int Llamadas { get; private set; }

    // La última petición recibida (para verificar método y URL).
    public HttpRequestMessage? UltimaSolicitud { get; private set; }

    public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respuesta)
    {
        _respuesta = respuesta;
    }

    // El servidor responde con este código HTTP y este cuerpo.
    public static FakeHttpMessageHandler Responde(HttpStatusCode codigo, string contenido = "", string tipoContenido = "application/json")
        => new((_, _) => Task.FromResult(new HttpResponseMessage(codigo)
        {
            Content = new StringContent(contenido, Encoding.UTF8, tipoContenido)
        }));

    // La comunicación falla con esta excepción (sin red, timeout, etc.).
    public static FakeHttpMessageHandler Lanza(Exception excepcion)
        => new((_, _) => Task.FromException<HttpResponseMessage>(excepcion));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Llamadas++;
        UltimaSolicitud = request;
        return _respuesta(request, cancellationToken);
    }
}
