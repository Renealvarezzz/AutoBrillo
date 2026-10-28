using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Repository.Repositorios;

public interface ITipoProductoRepository
{
    Task<List<TipoProducto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<TipoProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> TieneProductosAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(TipoProducto tipo, CancellationToken cancellationToken = default);
    void Eliminar(TipoProducto tipo);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
