namespace EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels
{
    // ==============================================================================================
    // Contrato para los ViewModels que reciben datos al navegar hacia su pantalla.
    // Reemplaza a IQueryAttributable (un tipo de MAUI) para que los ViewModels no dependan del
    // framework de UI. Quien lo conecta con Shell es la página (ver ContactoModalPage y
    // UsuarioApiDetallePage), que implementa IQueryAttributable y reenvía los parámetros.
    // ==============================================================================================
    public interface IRecibeParametros
    {
        void RecibirParametros(IDictionary<string, object> parametros);
    }
}