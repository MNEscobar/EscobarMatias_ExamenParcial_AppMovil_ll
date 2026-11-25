namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Abstrae la vibración del dispositivo (en MAUI, Vibration). La implementación real está en la app
    // (VibracionService). Contrato: NUNCA lanza excepciones; si el dispositivo no vibra, no hace nada.
    public interface IVibracionService
    {
        // Hace vibrar el dispositivo durante la duración indicada.
        // (En iOS el sistema ignora la duración y usa un valor fijo.)
        void Vibrar(TimeSpan duracion);
    }
}