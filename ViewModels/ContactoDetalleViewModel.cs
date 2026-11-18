using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EscobarMatias_ActividadPractica_AppMovil_ll.Data;
using EscobarMatias_ActividadPractica_AppMovil_ll.Helpers;
using EscobarMatias_ActividadPractica_AppMovil_ll.Models;
using EscobarMatias_ActividadPractica_AppMovil_ll.Services;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels
{
    // ========================================================================================
    // ViewModel de la pantalla modal de Alta/Edición de contacto.
    // Implementa IRecibeParametros para recibir el Contacto que envía ContactosViewModel
    // (la página lo conecta con Shell mediante IQueryAttributable y le reenvía los parámetros).
    // Esto reemplaza la necesidad de acceder a la base de datos desde acá constantemente.
    // ========================================================================================

    public partial class ContactoDetalleViewModel : ObservableObject, IRecibeParametros
    {
        private readonly IContactoRepository _contactoRepository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private int _idContacto;

        [ObservableProperty]
        private string nombre = string.Empty;

        [ObservableProperty]
        private string telefono = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string errorNombre = string.Empty;

        [ObservableProperty]
        private string errorTelefono = string.Empty;

        [ObservableProperty]
        private string errorEmail = string.Empty;

        [ObservableProperty]
        private string tituloPagina = "Nuevo contacto";

        public ContactoDetalleViewModel(
            IContactoRepository contactoRepository,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _contactoRepository = contactoRepository;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        // Se llama a este método apenas se navega a esta pantalla, con el diccionario de parámetros.
        // Acá se "desempaqueta" el Contacto recibido y se cargan los campos editables del formulario.
        public void RecibirParametros(IDictionary<string, object> parametros)
        {
            if (parametros.TryGetValue(RutasNavegacion.ParametroContacto, out var valor) && valor is Contacto contactoRecibido)
            {
                _idContacto = contactoRecibido.Id;
                Nombre = contactoRecibido.Nombre;
                Telefono = contactoRecibido.Telefono;
                Email = contactoRecibido.Email;

                TituloPagina = _idContacto == 0 ? "Nuevo contacto" : "Editar contacto";
            }
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (!ValidarCampos())
                return;

            try
            {
                var contacto = new Contacto
                {
                    Id = _idContacto,
                    Nombre = Nombre.Trim(),
                    Telefono = Telefono.Trim(),
                    Email = Email.Trim()
                };

                // _idContacto == 0  -> todavía no existe en la base -> Insertar.
                // _idContacto != 0  -> ya existe -> Actualizar (nombre/teléfono/email).
                bool operacionExitosa = _idContacto == 0
                    ? await _contactoRepository.InsertarAsync(contacto)
                    : await _contactoRepository.ActualizarAsync(contacto);

                if (operacionExitosa)
                    await CerrarModalAsync();
                else
                    await _dialogService.MostrarAlertaAsync("Error", "No se pudo guardar el contacto.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ContactoDetalleViewModel] {ex}");
                await _dialogService.MostrarAlertaAsync("Error", "Ocurrió un error al guardar el contacto.");
            }
        }

        // Cierra el modal sin guardar ningún cambio.
        [RelayCommand]
        private async Task CancelarAsync()
        {
            await CerrarModalAsync();
        }

        private Task CerrarModalAsync() => _navigationService.VolverAsync();

        // Delega las reglas en ValidacionHelper (única fuente de las validaciones y de sus mensajes)
        // y copia el resultado a las propiedades que la pantalla muestra en rojo.
        private bool ValidarCampos()
        {
            var resultado = ValidacionHelper.ValidarContacto(Nombre, Telefono, Email);

            ErrorNombre = resultado.ErrorNombre;
            ErrorTelefono = resultado.ErrorTelefono;
            ErrorEmail = resultado.ErrorEmail;

            return resultado.EsValido;
        }
    }
}