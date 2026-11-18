using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EscobarMatias_ActividadPractica_AppMovil_ll.Data;
using EscobarMatias_ActividadPractica_AppMovil_ll.Models;
using EscobarMatias_ActividadPractica_AppMovil_ll.Services;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels
{
    // =============================================================================
    // ViewModel de la pantalla principal (lista de contactos).
    // No conoce SQLite ni ninguna clase de "SQLite.*": toda la persistencia
    // pasa por IContactoRepository, que llega inyectado por constructor
    // (el registro de la dependencia está en MauiProgram.cs).
    // Los diálogos (IDialogService) y la navegación (INavigationService) también se
    // reciben como abstracciones: el ViewModel no depende de Shell ni de ninguna
    // clase de la interfaz gráfica, y por eso se puede probar con mocks.
    // =============================================================================

    public partial class ContactosViewModel : ObservableObject
    {
        private readonly IContactoRepository _contactoRepository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        public ObservableCollection<Contacto> Contactos { get; } = new();

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        [ObservableProperty]
        private bool estaCargando;

        public ContactosViewModel(
            IContactoRepository contactoRepository,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _contactoRepository = contactoRepository;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task CargarContactosAsync()
        {
            try
            {
                EstaCargando = true;

                var listaDesdeDb = await _contactoRepository.ObtenerTodosAsync();

                Contactos.Clear();
                foreach (var contacto in listaDesdeDb)
                    Contactos.Add(contacto);
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo cargar la lista de contactos.", ex);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task BuscarAsync()
        {
            try
            {
                EstaCargando = true;

                var resultado = await _contactoRepository.BuscarPorNombreAsync(TextoBusqueda);

                Contactos.Clear();
                foreach (var contacto in resultado)
                    Contactos.Add(contacto);
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo realizar la búsqueda.", ex);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task AgregarContactoAsync()
        {
            await NavegarAlModalAsync(new Contacto());
        }

        [RelayCommand]
        private async Task EditarContactoAsync(Contacto contacto)
        {
            if (contacto is null)
                return;

            await NavegarAlModalAsync(contacto);
        }

        // Abre el modal de alta/edición enviando el contacto como parámetro.
        // La ruta y la clave del parámetro salen de RutasNavegacion (sin "strings mágicos").
        private async Task NavegarAlModalAsync(Contacto contacto)
        {
            try
            {
                var parametrosNavegacion = new Dictionary<string, object>
                {
                    { RutasNavegacion.ParametroContacto, contacto }
                };

                await _navigationService.NavegarAAsync(RutasNavegacion.ContactoModal, parametrosNavegacion);
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("No se pudo abrir la pantalla de contacto.", ex);
            }
        }

        [RelayCommand]
        private async Task EliminarContactoAsync(Contacto contacto)
        {
            if (contacto is null)
                return;

            try
            {
                bool confirmar = await _dialogService.ConfirmarAsync(
                    "Eliminar contacto",
                    $"¿Seguro que querés eliminar a {contacto.Nombre}?",
                    "Eliminar",
                    "Cancelar");

                if (!confirmar)
                    return;

                bool eliminado = await _contactoRepository.EliminarAsync(contacto.Id);

                if (eliminado)
                    await CargarContactosAsync();
                else
                    await _dialogService.MostrarAlertaAsync("Aviso", "No se pudo eliminar el contacto.");
            }
            catch (Exception ex)
            {
                await MostrarErrorAsync("Ocurrió un error al eliminar el contacto.", ex);
            }
        }

        private async Task MostrarErrorAsync(string mensajeUsuario, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ContactosViewModel] {ex}");
            await _dialogService.MostrarAlertaAsync("Error", mensajeUsuario);
        }
    }
}