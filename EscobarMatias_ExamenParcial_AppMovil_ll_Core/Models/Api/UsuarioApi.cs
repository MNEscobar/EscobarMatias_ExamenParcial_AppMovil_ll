using System.Globalization;
using System.Text.Json.Serialization;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models.Api
{
    // ==============================================================================================
    // DTO (Data Transfer Object) que representa un usuario tal como lo devuelve la API pública
    // https://jsonplaceholder.typicode.com/users
    //
    // Los nombres de las propiedades C# están en español, pero el JSON de la API está en inglés:
    // [JsonPropertyName] indica a System.Text.Json con qué campo del JSON se corresponde cada una.
    // Los objetos anidados del JSON ("company", "address" y, dentro de éste, "geo") se modelan con sus propias clases.
    // ==============================================================================================
    public class UsuarioApi
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string NombreUsuario { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("website")]
        public string SitioWeb { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public EmpresaApi Empresa { get; set; } = new();

        [JsonPropertyName("address")]
        public DireccionApi Direccion { get; set; } = new();
    }

    // Objeto anidado "company" del JSON.
    public class EmpresaApi
    {
        [JsonPropertyName("name")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("catchPhrase")]
        public string Eslogan { get; set; } = string.Empty;
    }

    // Objeto anidado "address" del JSON.
    public class DireccionApi
    {
        [JsonPropertyName("street")]
        public string Calle { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string Ciudad { get; set; } = string.Empty;

        // Objeto anidado "geo" (coordenadas) dentro de "address".
        [JsonPropertyName("geo")]
        public GeoApi Geo { get; set; } = new();
    }

    // Objeto anidado "geo" del JSON. JSONPlaceholder devuelve las coordenadas como TEXTO
    // (por ejemplo "lat": "-37.3159"), por eso se modelan como string y se convierten a número
    // con ObtenerUbicacion(), que usa la cultura invariante (el punto es el separador decimal).
    public class GeoApi
    {
        [JsonPropertyName("lat")]
        public string Latitud { get; set; } = string.Empty;

        [JsonPropertyName("lng")]
        public string Longitud { get; set; } = string.Empty;

        // Convierte las coordenadas a un objeto Ubicacion.
        // Devuelve null si faltan, no son números o están fuera del rango geográfico válido.
        // (Es un método y no una propiedad a propósito: así System.Text.Json no lo trata como parte del JSON.)
        public Ubicacion? ObtenerUbicacion()
        {
            if (!double.TryParse(Latitud, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitud) ||
                !double.TryParse(Longitud, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitud))
                return null;

            var ubicacion = new Ubicacion(latitud, longitud);
            return ubicacion.EsValida ? ubicacion : null;
        }
    }
}
