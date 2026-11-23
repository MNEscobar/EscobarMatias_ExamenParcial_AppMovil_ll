using SQLite;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Integration;

// ==============================================================================================
// Pruebas de INTEGRACIÓN: el ContactoRepository real contra SQLite real, pero EN MEMORIA (":memory:"):
// no se crea ningún archivo y cada prueba arranca con una base limpia.
//
// Aislamiento entre pruebas:
//  sqlite-net guarda las conexiones en un pool estático cuya clave es la ruta de la base. Por eso todos
//  los repositorios creados con ":memory:" comparten UNA sola base en memoria mientras el pool no se
//  limpie. (Una URI distinta por prueba no sirve con el constructor real del repositorio, que abre la
//  base con las opciones por defecto, sin el modo URI.) Se resuelve así:
//   1) [Collection("SQLite")]: las pruebas de esta clase se ejecutan de a una, nunca en paralelo.
//   2) Dispose(): al terminar CADA prueba se llama a SQLiteAsyncConnection.ResetPool(), que cierra las
//      conexiones y descarta la base en memoria; la prueba siguiente empieza desde cero.
// ==============================================================================================
[Collection("SQLite")]
public class ContactoRepositoryTests : IDisposable
{
    private const string EnMemoria = ":memory:";

    private readonly List<string> _archivosTemporales = new();

    // xUnit crea una instancia nueva de la clase por cada prueba y llama a Dispose al terminar.
    public void Dispose()
    {
        SQLiteAsyncConnection.ResetPool();

        foreach (var archivo in _archivosTemporales)
        {
            try { File.Delete(archivo); }
            catch (Exception) { /* limpieza en el mejor esfuerzo: es una carpeta temporal */ }
        }
    }

    private static ContactoRepository CrearRepositorioEnMemoria() => new(EnMemoria);

    private string NuevaRutaTemporal()
    {
        string ruta = Path.Combine(Path.GetTempPath(), $"agenda_test_{Guid.NewGuid():N}.db3");
        _archivosTemporales.Add(ruta);
        return ruta;
    }

    // ------------------------ Inicialización y semilla ------------------------
    [Fact]
    public async Task ObtenerTodos_EnUnaBaseNueva_DevuelveLosTresContactosSembradosOrdenadosPorNombre()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        var contactos = await repositorio.ObtenerTodosAsync();

        // Assert
        Assert.Equal(3, contactos.Count);
        var nombres = contactos.Select(c => c.Nombre).ToList();
        Assert.Equal(nombres.OrderBy(n => n, StringComparer.Ordinal), nombres);
    }

    [Fact]
    public async Task ObtenerTodos_ConsultadoVariasVeces_NoDuplicaLaSemilla()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        await repositorio.ObtenerTodosAsync();
        await repositorio.ObtenerTodosAsync();
        var contactos = await repositorio.ObtenerTodosAsync();

        // Assert
        Assert.Equal(3, contactos.Count);
    }

    [Fact]
    public async Task ObtenerTodos_SiLaBaseYaTieneDatos_NoVuelveASembrar()
    {
        // Arrange: un primer repositorio crea la base y agrega un contacto.
        string ruta = NuevaRutaTemporal();
        var primero = new ContactoRepository(ruta);
        await primero.InsertarAsync(new Contacto { Nombre = "Zoe Díaz", Telefono = "342-5000000", Email = "zoe@mail.com" });
        Assert.Equal(4, (await primero.ObtenerTodosAsync()).Count);

        // Act: un segundo repositorio sobre el mismo archivo simula "volver a abrir la app".
        var segundo = new ContactoRepository(ruta);
        var contactos = await segundo.ObtenerTodosAsync();

        // Assert
        Assert.Equal(4, contactos.Count);
    }

    // ------------------------------ Insertar ------------------------------
    [Fact]
    public async Task Insertar_AsignaUnIdYAgregaElContacto()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();
        var nuevo = new Contacto { Nombre = "Zoe Díaz", Telefono = "342-5000000", Email = "zoe@mail.com" };

        // Act
        bool insertado = await repositorio.InsertarAsync(nuevo);

        // Assert
        Assert.True(insertado);
        Assert.True(nuevo.Id > 0);
        var contactos = await repositorio.ObtenerTodosAsync();
        Assert.Equal(4, contactos.Count);
        Assert.Contains(contactos, c => c.Id == nuevo.Id && c.Email == "zoe@mail.com");
    }

    // ----------------------------- Actualizar -----------------------------
    [Fact]
    public async Task Actualizar_PersisteLosCambiosDelContacto()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();
        var contacto = (await repositorio.ObtenerTodosAsync()).First();
        contacto.Telefono = "342-5999999";
        contacto.Email = "nuevo@mail.com";

        // Act
        bool actualizado = await repositorio.ActualizarAsync(contacto);

        // Assert
        Assert.True(actualizado);
        var recargado = (await repositorio.ObtenerTodosAsync()).Single(c => c.Id == contacto.Id);
        Assert.Equal("342-5999999", recargado.Telefono);
        Assert.Equal("nuevo@mail.com", recargado.Email);
    }

    [Fact]
    public async Task Actualizar_UnContactoQueNoExiste_DevuelveFalse()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();
        var inexistente = new Contacto { Id = 9999, Nombre = "Nadie" };

        // Act
        bool actualizado = await repositorio.ActualizarAsync(inexistente);

        // Assert
        Assert.False(actualizado);
    }

    // ------------------------------ Eliminar ------------------------------
    [Fact]
    public async Task Eliminar_UnContactoExistente_LoQuitaDeLaBase()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();
        var contacto = (await repositorio.ObtenerTodosAsync()).First();

        // Act
        bool eliminado = await repositorio.EliminarAsync(contacto.Id);

        // Assert
        Assert.True(eliminado);
        var restantes = await repositorio.ObtenerTodosAsync();
        Assert.Equal(2, restantes.Count);
        Assert.DoesNotContain(restantes, c => c.Id == contacto.Id);
    }

    [Fact]
    public async Task Eliminar_UnIdQueNoExiste_DevuelveFalseYNoTocaLaBase()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        bool eliminado = await repositorio.EliminarAsync(9999);

        // Assert
        Assert.False(eliminado);
        Assert.Equal(3, (await repositorio.ObtenerTodosAsync()).Count);
    }

    // ------------------------------- Buscar -------------------------------
    [Fact]
    public async Task BuscarPorNombre_AceptaUnaCoincidenciaParcial()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        var resultado = await repositorio.BuscarPorNombreAsync("uan");

        // Assert
        var encontrado = Assert.Single(resultado);
        Assert.Equal("Juan Pérez", encontrado.Nombre);
    }

    [Fact]
    public async Task BuscarPorNombre_NoDistingueMayusculasDeMinusculas()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act: lo que un usuario escribe normalmente, todo en minúscula.
        var resultado = await repositorio.BuscarPorNombreAsync("ana");

        // Assert
        var encontrado = Assert.Single(resultado);
        Assert.Equal("Ana García", encontrado.Nombre);
    }

    [Fact]
    public async Task BuscarPorNombre_SinCoincidencias_DevuelveUnaListaVacia()
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        var resultado = await repositorio.BuscarPorNombreAsync("xyz");

        // Assert
        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BuscarPorNombre_ConTextoVacio_DevuelveTodosLosContactos(string texto)
    {
        // Arrange
        var repositorio = CrearRepositorioEnMemoria();

        // Act
        var resultado = await repositorio.BuscarPorNombreAsync(texto);

        // Assert
        Assert.Equal(3, resultado.Count);
    }

    // ------------------- Manejo de errores (try-catch) -------------------
    [Fact]
    public async Task ConUnaRutaInvalida_ObtenerTodosDevuelveListaVacia_SinLanzarExcepcion()
    {
        // Arrange: la carpeta no existe, así que SQLite no puede crear el archivo.
        string rutaInvalida = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "no_existe", "agenda.db3");
        var repositorio = new ContactoRepository(rutaInvalida);

        // Act
        var contactos = await repositorio.ObtenerTodosAsync();

        // Assert
        Assert.Empty(contactos);
    }

    [Fact]
    public async Task ConUnaRutaInvalida_LasEscriturasDevuelvenFalse_SinLanzarExcepcion()
    {
        // Arrange
        string rutaInvalida = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "no_existe", "agenda.db3");
        var repositorio = new ContactoRepository(rutaInvalida);

        // Act
        bool insertado = await repositorio.InsertarAsync(new Contacto { Nombre = "Ana" });
        bool actualizado = await repositorio.ActualizarAsync(new Contacto { Id = 1, Nombre = "Ana" });
        bool eliminado = await repositorio.EliminarAsync(1);

        // Assert
        Assert.False(insertado);
        Assert.False(actualizado);
        Assert.False(eliminado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ConUnaRutaVacia_LanzaArgumentException(string ruta)
    {
        // Arrange: la ruta inválida llega por [InlineData].

        // Act
        var accion = () => new ContactoRepository(ruta);

        // Assert
        Assert.ThrowsAny<ArgumentException>(accion);
    }
}
