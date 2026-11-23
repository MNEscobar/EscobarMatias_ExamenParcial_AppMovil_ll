namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Unit.Helpers;

// Pruebas unitarias de las validaciones. Son funciones puras: no necesitan mocks ni base de datos.
// Todas siguen el patrón AAA (Arrange, Act, Assert).
public class ValidacionHelperTests
{
    // ------------------------------- Nombre -------------------------------
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Ana", true)]
    [InlineData("  Ana García  ", true)]
    public void EsNombreValido_CasosBasicos(string? nombre, bool esperado)
    {
        // Arrange: el nombre y el resultado esperado llegan por [InlineData].

        // Act
        bool resultado = ValidacionHelper.EsNombreValido(nombre);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void EsNombreValido_AceptaExactamenteLaLongitudMaxima()
    {
        // Arrange
        string nombre = new('a', Contacto.LongitudMaximaNombre);

        // Act
        bool resultado = ValidacionHelper.EsNombreValido(nombre);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void EsNombreValido_RechazaUnCaracterDeMas()
    {
        // Arrange
        string nombre = new('a', Contacto.LongitudMaximaNombre + 1);

        // Act
        bool resultado = ValidacionHelper.EsNombreValido(nombre);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void EsNombreValido_IgnoraLosEspaciosDeLosExtremosAlMedirLaLongitud()
    {
        // Arrange
        string nombre = "  " + new string('a', Contacto.LongitudMaximaNombre) + "  ";

        // Act
        bool resultado = ValidacionHelper.EsNombreValido(nombre);

        // Assert
        Assert.True(resultado);
    }

    // ------------------------------ Teléfono ------------------------------
    [Theory]
    [InlineData("342-5123456", true)]        // 10 dígitos con guion
    [InlineData("+54 9 342 5123456", true)]  // formato internacional
    [InlineData("(342) 512-3456", true)]     // con paréntesis y espacios
    [InlineData("1234567", true)]            // mínimo: 7 dígitos
    [InlineData("123456789012345", true)]    // máximo: 15 dígitos
    [InlineData("123456", false)]            // 6 dígitos: demasiado corto
    [InlineData("1234567890123456", false)]  // 16 dígitos: demasiado largo
    [InlineData("abc1234567", false)]        // contiene letras
    [InlineData("342.5123456", false)]       // el punto no está permitido
    [InlineData("++5434251234", false)]      // "+" repetido
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void EsTelefonoValido_CasosBasicos(string? telefono, bool esperado)
    {
        // Arrange: el teléfono y el resultado esperado llegan por [InlineData].

        // Act
        bool resultado = ValidacionHelper.EsTelefonoValido(telefono);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    // -------------------------------- Email -------------------------------
    [Theory]
    [InlineData("ana@mail.com", true)]
    [InlineData("  ana.garcia@mail.com  ", true)]
    [InlineData("ana@mail.com.ar", true)]
    [InlineData("ana@mail", false)]          // falta el punto del dominio
    [InlineData("ana.mail.com", false)]      // falta la arroba
    [InlineData("ana @mail.com", false)]     // espacio en el medio
    [InlineData("@mail.com", false)]         // falta el usuario
    [InlineData("ana@@mail.com", false)]     // arroba duplicada
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsEmailValido_CasosBasicos(string? email, bool esperado)
    {
        // Arrange: el email y el resultado esperado llegan por [InlineData].

        // Act
        bool resultado = ValidacionHelper.EsEmailValido(email);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    // --------------------- ValidarContacto (los tres campos) ---------------------
    [Fact]
    public void ValidarContacto_ConDatosCorrectos_EsValidoYSinMensajes()
    {
        // Arrange
        string nombre = "Ana García", telefono = "342-5123456", email = "ana@mail.com";

        // Act
        var resultado = ValidacionHelper.ValidarContacto(nombre, telefono, email);

        // Assert
        Assert.True(resultado.EsValido);
        Assert.Equal(string.Empty, resultado.ErrorNombre);
        Assert.Equal(string.Empty, resultado.ErrorTelefono);
        Assert.Equal(string.Empty, resultado.ErrorEmail);
    }

    [Fact]
    public void ValidarContacto_ConTodoVacio_DevuelveUnMensajePorCampo()
    {
        // Arrange
        string vacio = "";

        // Act
        var resultado = ValidacionHelper.ValidarContacto(vacio, vacio, vacio);

        // Assert
        Assert.False(resultado.EsValido);
        Assert.Contains("obligatorio", resultado.ErrorNombre);
        Assert.Contains("obligatorio", resultado.ErrorTelefono);
        Assert.NotEqual(string.Empty, resultado.ErrorEmail);
    }

    [Fact]
    public void ValidarContacto_ConNombreMuyLargo_MencionaElLimite()
    {
        // Arrange
        string nombre = new('a', Contacto.LongitudMaximaNombre + 1);

        // Act
        var resultado = ValidacionHelper.ValidarContacto(nombre, "342-5123456", "ana@mail.com");

        // Assert
        Assert.False(resultado.EsValido);
        Assert.Contains(Contacto.LongitudMaximaNombre.ToString(), resultado.ErrorNombre);
        Assert.Equal(string.Empty, resultado.ErrorTelefono);
        Assert.Equal(string.Empty, resultado.ErrorEmail);
    }

    [Fact]
    public void ValidarContacto_ConTelefonoInvalido_MencionaLaCantidadDeDigitos()
    {
        // Arrange
        string telefonoInvalido = "abc";

        // Act
        var resultado = ValidacionHelper.ValidarContacto("Ana", telefonoInvalido, "ana@mail.com");

        // Assert
        Assert.False(resultado.EsValido);
        Assert.Contains(ValidacionHelper.MinimoDigitosTelefono.ToString(), resultado.ErrorTelefono);
        Assert.Contains(ValidacionHelper.MaximoDigitosTelefono.ToString(), resultado.ErrorTelefono);
        Assert.Equal(string.Empty, resultado.ErrorNombre);
    }

    [Fact]
    public void ValidarContacto_ConEmailInvalido_SoloFallaElEmail()
    {
        // Arrange
        string emailInvalido = "ana@mail";

        // Act
        var resultado = ValidacionHelper.ValidarContacto("Ana", "342-5123456", emailInvalido);

        // Assert
        Assert.False(resultado.EsValido);
        Assert.NotEqual(string.Empty, resultado.ErrorEmail);
        Assert.Equal(string.Empty, resultado.ErrorNombre);
        Assert.Equal(string.Empty, resultado.ErrorTelefono);
    }
}
