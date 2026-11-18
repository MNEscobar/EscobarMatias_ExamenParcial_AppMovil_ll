namespace EscobarMatias_ActividadPractica_AppMovil_ll.Services
{
    // ==============================================================================================
    // Constantes de navegación: nombres de las rutas registradas en Shell y claves de los
    // parámetros que viajan entre pantallas. Centralizarlas evita "strings mágicos" repetidos
    // (y errores de tipeo) y permite que un ViewModel navegue sin conocer la clase de la página
    // de destino, que pertenece a la capa de presentación.
    // ==============================================================================================
    public static class RutasNavegacion
    {
        // Rutas (se registran con Routing.RegisterRoute en AppShell.xaml.cs).
        public const string ContactoModal = "ContactoModalPage";
        public const string UsuarioApiDetalle = "UsuarioApiDetallePage";

        // Claves de los parámetros de navegación.
        public const string ParametroContacto = "Contacto";
        public const string ParametroUsuario = "Usuario";
    }
}