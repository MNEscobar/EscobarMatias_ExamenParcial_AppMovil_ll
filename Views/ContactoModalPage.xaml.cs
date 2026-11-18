using EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.Views
{
    // Debe ser partial para coincidir con la declaración generada por XAML.
    // Implementa IQueryAttributable porque Shell entrega los parámetros de navegación a la página:
    // ella solo los reenvía al ViewModel (a través de IRecibeParametros) sin interpretarlos.
    public partial class ContactoModalPage : ContentPage, IQueryAttributable
    {
        // Igual que ContactosPage, el ViewModel llega inyectado desde el
        // contenedor de DI armado en MauiProgram.cs.
        public ContactoModalPage(ContactoDetalleViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (BindingContext is IRecibeParametros receptor)
                receptor.RecibirParametros(query);
        }
    }
}