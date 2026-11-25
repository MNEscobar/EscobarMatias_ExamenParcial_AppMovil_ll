# Agenda de Contactos — .NET MAUI

Aplicación móvil desarrollada en **.NET MAUI (.NET 9.0)** para la asignatura **Desarrollo de Aplicaciones Móviles II (UCES — Tecnicatura en Programación)**. Combina persistencia local con **SQLite**, consumo de una **API REST** (JSONPlaceholder), **sensores nativos** (GPS y vibración con permisos en tiempo de ejecución), una arquitectura desacoplada en capas (**App MAUI + Core + Tests**), **193 pruebas automatizadas** con **xUnit + NSubstitute** y un pipeline de **CI/CD en GitHub Actions**.

## Funcionalidades principales

### Agenda de Contactos (SQLite local)
- **CRUD completo:** listar, buscar por nombre, agregar, editar y eliminar contactos con persistencia local mediante `sqlite-net-pcl` (`SQLiteAsyncConnection`).
- **Búsqueda insensible a mayúsculas:** filtrado en tiempo real desde `SearchBar` normalizando texto con `ToLower()` / `ToLowerInvariant()`.
- **Siembra inicial (Seed):** carga automática de 3 contactos iniciales únicamente cuando la tabla está vacía.
- **Navegación modal:** creación y edición mediante `ContactoModalPage` (`Shell.PresentationMode="ModalAnimated"`), evitando recargas innecesarias de la lista principal.
- **Validaciones centralizadas:** reglas puras en `ValidacionHelper` (nombre obligatorio hasta 50 caracteres, formato de teléfono y formato de correo electrónico) con feedback visual en la interfaz.

### Usuarios API y Sensores nativos
- **Consumo de API REST:** consulta de usuarios desde `https://jsonplaceholder.typicode.com/` mediante `HttpClient` administrado con `AddHttpClient` (`Timeout` de 10 segundos), verificación previa de conectividad y tipado de errores (`ApiResult` / `TipoErrorApi`).
- **GPS y cálculo de distancia:** desde el detalle de un usuario (`UsuarioApiDetallePage`), solicita permisos de ubicación en tiempo de ejecución (`LocationWhenInUse`), obtiene las coordenadas actuales del dispositivo con `Geolocation.Default` y calcula la distancia en kilómetros hasta las coordenadas `geo` (`lat`/`lng`) del usuario aplicando la fórmula de **Haversine** (`GeoHelper`).
- **Vibración ante errores:** feedback háptico mediante `Vibration.Default` desde `UsuariosViewModel` cuando falla una solicitud a la API (verificando `IsSupported` y encapsulado en `try/catch`).

## Arquitectura en 3 proyectos

La solución separa la lógica de negocio y presentación testeable en una librería `net9.0` pura (**sin dependencias de .NET MAUI**), lo que permite ejecutar toda la suite de pruebas y el pipeline de CI/CD sin emulador ni workloads móviles.

```text
EscobarMatias_ExamenParcial_AppMovil_ll/
├── .github/
│   └── workflows/
│       └── ci.yml                                       # Pipeline CI/CD (matriz, caché, tests, cobertura y artefactos)
├── EscobarMatias_ExamenParcial_AppMovil_ll.sln          # Solución con los 3 proyectos
├── EscobarMatias_ExamenParcial_AppMovil_ll.csproj       # App MAUI (Android, iOS, MacCatalyst, Windows)
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs                     # Rutas centralizadas con RutasNavegacion
├── MauiProgram.cs                                       # Contenedor central de Inyección de Dependencias
├── Helpers/
│   └── StringToBoolConverter.cs                         # IValueConverter para visibilidad de errores en XAML
├── Services/                                            # Implementaciones concretas de plataforma (MAUI)
│   ├── ConectividadService.cs
│   ├── DialogService.cs
│   ├── NavigationService.cs
│   ├── UbicacionService.cs                              # Geolocation + permisos runtime
│   └── VibracionService.cs                              # Vibration
├── Views/                                               # Interfaz XAML y code-behind mínimo
│   ├── ContactosPage.xaml(.cs)
│   ├── ContactoModalPage.xaml(.cs)                      # IQueryAttributable -> IRecibeParametros
│   ├── UsuariosPage.xaml(.cs)
│   └── UsuarioApiDetallePage.xaml(.cs)                  # IQueryAttributable + UI de distancia GPS
├── Platforms/
│   ├── Android/AndroidManifest.xml                      # Permisos de ubicación, VIBRATE y uses-feature
│   └── iOS/Info.plist                                   # NSLocationWhenInUseUsageDescription
│
├── EscobarMatias_ExamenParcial_AppMovil_ll_Core/        # Librería net9.0 sin dependencias de MAUI
│   ├── EscobarMatias_ExamenParcial_AppMovil_ll_Core.csproj
│   ├── Data/
│   │   ├── IContactoRepository.cs
│   │   └── ContactoRepository.cs                        # SQLite asíncrono con ruta inyectada por constructor
│   ├── Helpers/
│   │   ├── ValidacionHelper.cs                          # Validaciones puras de nombre, teléfono y email
│   │   └── GeoHelper.cs                                 # Cálculo de distancia Haversine
│   ├── Models/
│   │   ├── Contacto.cs
│   │   ├── Ubicacion.cs
│   │   └── Api/UsuarioApi.cs                            # DTO con GeoApi (lat/lng)
│   ├── Services/                                        # Contratos (interfaces) y servicio HTTP
│   │   ├── ApiResult.cs / TipoErrorApi.cs
│   │   ├── IUsuarioApiService.cs / UsuarioApiService.cs
│   │   ├── IConectividadService.cs / IDialogService.cs / INavigationService.cs
│   │   ├── IUbicacionService.cs / ResultadoUbicacion.cs / IVibracionService.cs
│   │   └── RutasNavegacion.cs
│   └── ViewModels/
│       ├── ContactosViewModel.cs
│       ├── ContactoDetalleViewModel.cs
│       ├── UsuariosViewModel.cs
│       ├── UsuarioApiDetalleViewModel.cs
│       └── IRecibeParametros.cs
│
└── EscobarMatias_ExamenParcial_AppMovil_ll_Tests/       # Proyecto xUnit + NSubstitute (net9.0)
    ├── EscobarMatias_ExamenParcial_AppMovil_ll_Tests.csproj
    ├── Fakes/
    │   └── FakeHttpMessageHandler.cs                    # Simulación HTTP sin red real
    ├── Integration/
    │   └── ContactoRepositoryTests.cs                   # SQLite In-Memory (":memory:")
    └── Unit/
        ├── Helpers/
        │   ├── ValidacionHelperTests.cs
        │   └── GeoHelperTests.cs
        ├── Models/
        │   └── UsuarioApiTests.cs
        ├── Services/
        │   ├── UsuarioApiServiceTests.cs
        │   └── ResultadoUbicacionTests.cs
        └── ViewModels/
            ├── ContactosViewModelTests.cs
            ├── ContactoDetalleViewModelTests.cs
            ├── UsuariosViewModelTests.cs
            └── UsuarioApiDetalleViewModelTests.cs
```

## Tecnologías y librerías

| Capa | Tecnologías / Paquetes |
| :--- | :--- |
| **App MAUI** | .NET MAUI (`.NET 9.0`), C# / XAML, `Microsoft.Extensions.Http` (`9.0.8`), `Microsoft.Extensions.Logging.Debug` (`9.0.8`) |
| **Core (`net9.0`)** | `CommunityToolkit.Mvvm` (`8.4.2`), `sqlite-net-pcl` (`1.11.285`), `SQLitePCLRaw.bundle_green` (`2.1.11`) |
| **Testing (`net9.0`)** | `xunit` (`2.9.3`), `xunit.runner.visualstudio` (`2.8.2`), `Microsoft.NET.Test.Sdk` (`17.14.1`), `NSubstitute` (`5.3.0`), `coverlet.collector` (`6.0.4`) |
| **CI/CD** | GitHub Actions (`checkout@v7`, `setup-dotnet@v6`, `cache@v6`, `upload-artifact@v7`, `ReportGenerator`) |

## Testing (193 pruebas automatizadas)

Todas las pruebas siguen el patrón **AAA (Arrange, Act, Assert)**:
- **Pruebas unitarias de helpers y modelos:** `ValidacionHelperTests`, `GeoHelperTests` (Haversine y casos de borde) y `UsuarioApiTests` (deserialización de coordenadas `geo`).
- **Pruebas unitarias de servicios:** `UsuarioApiServiceTests` con `FakeHttpMessageHandler` y mock de `IConectividadService` (códigos 200, 400, 404, 500, timeout, JSON inválido y sin conexión) y `ResultadoUbicacionTests`.
- **Pruebas unitarias de ViewModels:** los 4 ViewModels probados en aislamiento con **NSubstitute** sobre `IContactoRepository`, `IUsuarioApiService`, `INavigationService`, `IDialogService`, `IUbicacionService` e `IVibracionService`.
- **Pruebas de integración con SQLite In-Memory:** `ContactoRepositoryTests` contra `":memory:"`, aisladas con `[Collection("SQLite")]` y limpieza del pool mediante `SQLiteAsyncConnection.ResetPool()`.

### Ejecutar las pruebas localmente

Ejecutá `dotnet test` apuntando al proyecto de pruebas (no requiere workloads de Android/iOS):

```bash
dotnet test ./EscobarMatias_ExamenParcial_AppMovil_ll_Tests/EscobarMatias_ExamenParcial_AppMovil_ll_Tests.csproj
```

## CI/CD con GitHub Actions

El workflow [.github/workflows/ci.yml](https://www.google.com/search?q=.github/workflows/ci.yml) se ejecuta en cada `push` y `pull_request` hacia `main` (y manualmente vía `workflow_dispatch`):
1. **Job `pruebas` (matriz 2×2):** corre en `ubuntu-latest` y `windows-latest` para `Debug` y `Release`, con caché de NuGet (`actions/cache@v6`), restauración, compilación (`--no-restore`), pruebas (`--no-build`) con salida `.trx` y cobertura (`XPlat Code Coverage`), reporte HTML/Markdown en el *Summary* con `ReportGenerator` y subida de artefactos (`actions/upload-artifact@v7`).
2. **Job `publicar-core`:** si las 4 combinaciones de pruebas pasan en verde, publica los binarios Release de `_Core` y los sube como artefacto (`core-net9.0-publicado`).

## Cómo ejecutar la aplicación

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/MNEscobar/EscobarMatias_ExamenParcial_AppMovil_ll.git
   ```
2. Abrir `EscobarMatias_ExamenParcial_AppMovil_ll.sln` en Visual Studio 2022 (con el workload de **.NET Multi-platform App UI** instalado).
3. Restaurar paquetes NuGet y compilar la solución.
4. Seleccionar la plataforma de destino (Android, Windows, iOS) y ejecutar.
   - **Emulador Android (GPS):** configurar una ubicación simulada en *Extended controls → Location*.
   - **Vibración:** probar en un dispositivo físico (por ejemplo activando modo avión y presionando *Cargar usuarios*).