using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public class RepositorioProductos(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IProductoRepository
{
    public Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => contexto.Productos.Include(x => x.Tipos).OrderBy(x => x.Descripcion).ToListAsync(cancellationToken);
    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => contexto.Productos.Include(x => x.Tipos).SingleOrDefaultAsync(x => x.Id_Productos == id, cancellationToken);
    public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => contexto.Productos.AnyAsync(x => x.Descripcion == descripcion && (!excluirId.HasValue || x.Id_Productos != excluirId), cancellationToken);
    public Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default) => contexto.Productos.AddAsync(producto, cancellationToken).AsTask();

    public async Task<Producto> CrearConStockInicialAsync(Producto producto, decimal stockInicial, int idUsuario, CancellationToken cancellationToken = default)
    {
        if (stockInicial < 0 || stockInicial != decimal.Truncate(stockInicial))
            throw new ReglaNegocioException("El stock inicial debe ser un entero no negativo.");
        producto.StockActual = 0;
        contexto.Productos.Add(producto);
        await using var tx = await contexto.Database.BeginTransactionAsync(cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        if (stockInicial > 0)
        {
            await operativo.MoverStockAsync(producto.Id_Productos, ReglasNegocio.Entrada, "Ajuste", stockInicial, null, "Stock inicial", idUsuario, cancellationToken, forzar: true);
            await contexto.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return producto;
    }

    public void Eliminar(Producto producto) => contexto.Productos.Remove(producto);
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => contexto.SaveChangesAsync(cancellationToken);
}
