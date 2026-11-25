using Microsoft.Extensions.Logging;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Data;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;
using EscobarMatias_ExamenParcial_AppMovil_ll_Core.ViewModels;
using EscobarMatias_ExamenParcial_AppMovil_ll.Services;
using EscobarMatias_ExamenParcial_AppMovil_ll.Views;

namespace EscobarMatias_ExamenParcial_AppMovil_ll
{
    public static class MauiProgram
    {
        // Dirección base de la API pública de pruebas (JSONPlaceholder).
        // Termina en "/" para que las rutas relativas del servicio ("users") se combinen bien.
        private const string UrlBaseApi = "https://jsonplaceholder.typicode.com/";

        // Nombre del archivo SQLite (se mantiene igual para no perder los datos ya guardados).
        private const string NombreArchivoBaseDeDatos = "agenda_contactos.db3";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ===================================================================
            // Inyección de dependencias: todo el "cableado" de la app vive acá,
            // Nada se instancia con "new" en Views ni en ViewModels; todo llega por constructor.
            // Los ViewModels piden interfaces (IContactoRepository, INavigationService, ...);
            // acá se decide qué implementación concreta recibe cada una.
            // ===================================================================

            // --- Persistencia local (SQLite) ---
            // Singleton: mantiene UNA sola conexión SQLiteAsyncConnection durante toda la vida de
            // la app. La ruta del archivo (que depende de la plataforma) se calcula acá, y no dentro
            // del repositorio, para que éste no dependa de MAUI. Se resuelve de forma perezosa:
            // FileSystem.AppDataDirectory se consulta la primera vez que alguien pide el repositorio.
            builder.Services.AddSingleton<IContactoRepository>(_ =>
                new ContactoRepository(Path.Combine(FileSystem.AppDataDirectory, NombreArchivoBaseDeDatos)));

            // --- Servicios de plataforma (Singleton: no guardan estado) ---
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IConectividadService, ConectividadService>();
            builder.Services.AddSingleton<IUbicacionService, UbicacionService>();   // GPS (Geolocation + permisos)
            builder.Services.AddSingleton<IVibracionService, VibracionService>();   // Vibración

            // --- API ---
            // AddHttpClient administra el ciclo de vida de HttpClient (evita agotar sockets)
            // y se lo inyecta ya configurado a UsuarioApiService (junto con IConectividadService).
            builder.Services.AddHttpClient<IUsuarioApiService, UsuarioApiService>(cliente =>
            {
                cliente.BaseAddress = new Uri(UrlBaseApi);
                cliente.Timeout = TimeSpan.FromSeconds(10);
            });

            // --- ViewModels y Páginas ---
            // Transient: cada vez que Shell navega a una de estas rutas, el contenedor crea
            // una instancia nueva. Es el comportamiento esperado para pantallas de
            // lista/formulario (así el modal siempre arranca limpio).
            builder.Services.AddTransient<ContactosViewModel>();
            builder.Services.AddTransient<ContactosPage>();

            builder.Services.AddTransient<ContactoDetalleViewModel>();
            builder.Services.AddTransient<ContactoModalPage>();

            builder.Services.AddTransient<UsuariosViewModel>();
            builder.Services.AddTransient<UsuariosPage>();

            builder.Services.AddTransient<UsuarioApiDetalleViewModel>();
            builder.Services.AddTransient<UsuarioApiDetallePage>();

            return builder.Build();
        }
    }
}