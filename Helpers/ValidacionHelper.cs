using System.Text.RegularExpressions;
using EscobarMatias_ActividadPractica_AppMovil_ll.Models;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.Helpers
{
    // ==============================================================================================
    // Validaciones de un contacto, centralizadas en un único lugar.
    // Son métodos puros: reciben datos simples (strings) y devuelven un resultado, sin depender de
    // la base de datos, de la interfaz ni de ningún servicio. Por eso la clase es estática, se
    // reutiliza desde cualquier ViewModel y se puede probar sin ninguna preparación.
    // ==============================================================================================
    public static class ValidacionHelper
    {
        // Un teléfono tiene entre 7 dígitos (número local) y 15 (máximo del estándar internacional E.164).
        public const int MinimoDigitosTelefono = 7;
        public const int MaximoDigitosTelefono = 15;

        // Patrón simple de email: algo@dominio.ext (sin espacios ni dobles arrobas).
        private static readonly Regex PatronEmail = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled);

        // Caracteres permitidos en un teléfono: un "+" inicial opcional, dígitos, espacios, guiones y paréntesis.
        private static readonly Regex PatronTelefono = new(
            @"^\+?[0-9\s\-()]+$",
            RegexOptions.Compiled);

        // El nombre no puede estar vacío ni superar la longitud máxima definida en Contacto.
        public static bool EsNombreValido(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return nombre.Trim().Length <= Contacto.LongitudMaximaNombre;
        }

        // El teléfono solo admite los caracteres del patrón y debe tener entre 7 y 15 dígitos.
        public static bool EsTelefonoValido(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return false;

            string texto = telefono.Trim();

            if (!PatronTelefono.IsMatch(texto))
                return false;

            int cantidadDeDigitos = texto.Count(c => c is >= '0' and <= '9');
            return cantidadDeDigitos is >= MinimoDigitosTelefono and <= MaximoDigitosTelefono;
        }

        // Verifica que el email tenga un formato válido (no necesariamente que exista).
        public static bool EsEmailValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return PatronEmail.IsMatch(email.Trim());
        }

        // Valida los tres campos de un contacto y devuelve un mensaje por cada campo inválido
        // (cadena vacía si el campo es correcto). Así los textos de error también viven acá
        // y el ViewModel solo los copia a las propiedades que la pantalla muestra.
        public static ResultadoValidacionContacto ValidarContacto(string? nombre, string? telefono, string? email)
        {
            string errorNombre = string.IsNullOrWhiteSpace(nombre)
                ? "El nombre es obligatorio."
                : !EsNombreValido(nombre)
                    ? $"El nombre no puede superar los {Contacto.LongitudMaximaNombre} caracteres."
                    : string.Empty;

            string errorTelefono = string.IsNullOrWhiteSpace(telefono)
                ? "El teléfono es obligatorio."
                : !EsTelefonoValido(telefono)
                    ? $"El teléfono debe tener entre {MinimoDigitosTelefono} y {MaximoDigitosTelefono} dígitos (solo números, espacios, guiones o paréntesis)."
                    : string.Empty;

            string errorEmail = EsEmailValido(email)
                ? string.Empty
                : "El email no tiene un formato válido.";

            return new ResultadoValidacionContacto(errorNombre, errorTelefono, errorEmail);
        }
    }

    // Resultado de validar un contacto: un mensaje por campo ("" = campo correcto).
    public sealed record ResultadoValidacionContacto(string ErrorNombre, string ErrorTelefono, string ErrorEmail)
    {
        public bool EsValido => ErrorNombre.Length == 0 && ErrorTelefono.Length == 0 && ErrorEmail.Length == 0;
    }
}