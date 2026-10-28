using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public class RepositorioProductos(AutoBrilloDbContext contexto) : IProductoRepository
{
    public Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => contexto.Productos.Include(x => x.Tipos).OrderBy(x => x.Descripcion).ToListAsync(cancellationToken);
    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => contexto.Productos.Include(x => x.Tipos).SingleOrDefaultAsync(x => x.Id_Productos == id, cancellationToken);
    public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => contexto.Productos.AnyAsync(x => x.Descripcion == descripcion && (!excluirId.HasValue || x.Id_Productos != excluirId), cancellationToken);
    public Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default) => contexto.Productos.AddAsync(producto, cancellationToken).AsTask();
    public void Eliminar(Producto producto) => contexto.Productos.Remove(producto);
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => contexto.SaveChangesAsync(cancellationToken);
}
