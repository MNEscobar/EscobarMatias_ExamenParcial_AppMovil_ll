using SQLite;

// ==============================================================================================
// Representa un contacto de la agenda.
// SQLite mapea directamente a una fila de la tabla "Contacto" en la base de datos local.
// Esta clase NO tiene lógica de negocio ni de validación: es solo la "forma" de los datos.
// La validación vive en Helpers/ValidacionHelper.cs
// y el acceso a la base de datos vive en Data/ContactoRepository.cs.
// ==============================================================================================

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models
{
    public class Contacto
    {
        // Longitud máxima del nombre. Es la única fuente de verdad: la usan tanto el atributo
        // [MaxLength] (esquema de SQLite) como ValidacionHelper (validación de la pantalla).
        public const int LongitudMaximaNombre = 50;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(LongitudMaximaNombre)]
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}