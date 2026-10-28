using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public class RepositorioTiposProductos(AutoBrilloDbContext contexto) : ITipoProductoRepository
{
    public Task<List<TipoProducto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => contexto.TiposProductos.OrderBy(x => x.Descripcion).ToListAsync(cancellationToken);
    public Task<TipoProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => contexto.TiposProductos.SingleOrDefaultAsync(x => x.Id_Tipos_Productos == id, cancellationToken);
    public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => contexto.TiposProductos.AnyAsync(x => x.Descripcion == descripcion && (!excluirId.HasValue || x.Id_Tipos_Productos != excluirId), cancellationToken);
    public Task<bool> TieneProductosAsync(int id, CancellationToken cancellationToken = default) => contexto.Productos.AnyAsync(x => x.Tipos.Any(tipo => tipo.Id_Tipos_Productos == id), cancellationToken);
    public Task AgregarAsync(TipoProducto tipo, CancellationToken cancellationToken = default) => contexto.TiposProductos.AddAsync(tipo, cancellationToken).AsTask();
    public void Eliminar(TipoProducto tipo) => contexto.TiposProductos.Remove(tipo);
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => contexto.SaveChangesAsync(cancellationToken);
}
