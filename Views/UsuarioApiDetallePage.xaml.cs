using EscobarMatias_ActividadPractica_AppMovil_ll.ViewModels;

namespace EscobarMatias_ActividadPractica_AppMovil_ll.Views;

// Igual que ContactoModalPage: recibe los parámetros de Shell (IQueryAttributable) y se los
// reenvía al ViewModel mediante IRecibeParametros, sin ninguna lógica propia.
public partial class UsuarioApiDetallePage : ContentPage, IQueryAttributable
{
    public UsuarioApiDetallePage(UsuarioApiDetalleViewModel viewModel)
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