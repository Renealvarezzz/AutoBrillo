using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Repository.Repositorios;

public interface IProductoRepository
{
    Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default);
    Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    void Eliminar(Producto producto);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
