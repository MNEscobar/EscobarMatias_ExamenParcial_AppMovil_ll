using System.Runtime.CompilerServices;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Tests.Support;

// Inicializa el proveedor nativo de SQLite una sola vez, apenas se carga el ensamblado de pruebas.
// (Llamarlo más de una vez es inofensivo.)
internal static class InicializadorSqlite
{
#pragma warning disable CA2255 // En un proyecto de pruebas es el lugar correcto para un inicializador de módulo
    [ModuleInitializer]
    internal static void Inicializar() => SQLitePCL.Batteries_V2.Init();
#pragma warning restore CA2255
}
