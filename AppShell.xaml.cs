using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services;
using EscobarMatias_ExamenParcial_AppMovil_ll.Views;

namespace EscobarMatias_ExamenParcial_AppMovil_ll
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rutas de las pantallas que no están en el TabBar y se abren con GoToAsync.
            // Los nombres salen de RutasNavegacion, la misma clase que usan los ViewModels.
            Routing.RegisterRoute(RutasNavegacion.ContactoModal, typeof(ContactoModalPage));
            Routing.RegisterRoute(RutasNavegacion.UsuarioApiDetalle, typeof(UsuarioApiDetallePage));
        }
    }
}
