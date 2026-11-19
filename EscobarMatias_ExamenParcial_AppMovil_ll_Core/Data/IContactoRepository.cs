using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Data
{
    // ==============================================================================================
    // Contrato del repositorio de contactos. Los ViewModels dependen de esta interfaz (no de la
    // clase concreta), por lo que en las pruebas se puede reemplazar por un mock sin tocar SQLite.
    // ==============================================================================================
    public interface IContactoRepository
    {
        // Devuelve todos los contactos, ordenados alfabéticamente por nombre.
        Task<List<Contacto>> ObtenerTodosAsync();

        // Busca contactos por nombre. Con texto vacío, devuelve todos.
        Task<List<Contacto>> BuscarPorNombreAsync(string textoBusqueda);

        // Inserta un contacto nuevo. Devuelve true si se guardó.
        Task<bool> InsertarAsync(Contacto contacto);

        // Actualiza un contacto existente (identificado por su Id). Devuelve true si se guardó.
        Task<bool> ActualizarAsync(Contacto contacto);

        // Elimina un contacto por Id. Devuelve true si existía y se eliminó.
        Task<bool> EliminarAsync(int id);
    }
}
