using EscobarMatias_ExamenParcial_AppMovil_ll_Core.Models.Api;

namespace EscobarMatias_ExamenParcial_AppMovil_ll_Core.Services
{
    // Contrato del servicio de API. Los ViewModels dependen de esta interfaz (no de la clase concreta),
    // lo que permite reemplazar el servicio por uno falso en pruebas.
    public interface IUsuarioApiService
    {
        Task<ApiResult<List<UsuarioApi>>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    }
}