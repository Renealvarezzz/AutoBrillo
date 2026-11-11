using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public interface IRepositorioCompras
{
    Task<List<Compra>> ObtenerTodasAsync(CancellationToken ct = default);
    Task<Compra> RegistrarAsync(Compra compra, int idUsuario, CancellationToken ct = default);
    Task AnularAsync(int id, string motivo, int idUsuario, CancellationToken ct = default);
}

/// <summary>PBI-006: cada compra suma stock. Anularla, solo el Administrador, lo revierte.</summary>
public class RepositorioCompras(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IRepositorioCompras
{
    public Task<List<Compra>> ObtenerTodasAsync(CancellationToken ct = default) =>
        contexto.Compras.AsNoTracking().Include(x => x.Proveedor).Include(x => x.Detalles).ThenInclude(d => d.Producto)
            .OrderByDescending(x => x.Fecha).ToListAsync(ct);

    public async Task<Compra> RegistrarAsync(Compra compra, int idUsuario, CancellationToken ct = default)
    {
        if (!await operativo.PersonaTieneTipoAsync(compra.Id_Personas, "Proveedor", ct))
            throw new ReglaNegocioException("Seleccioná un proveedor activo.");
        if (compra.Detalles.Count == 0) throw new ReglaNegocioException("La compra debe tener al menos un producto.");

        await using var transaccion = await contexto.Database.BeginTransactionAsync(ct);
        foreach (var d in compra.Detalles)
        {
            if (d.PrecioUnitario < 0 || d.PrecioUnitario != decimal.Truncate(d.PrecioUnitario))
                throw new ReglaNegocioException("El precio debe ser un entero no negativo.");
            var producto = await operativo.ObtenerProductoAsync(d.Id_Productos, ct);
            if (!ServicioOperativo.ManejaStock(producto))
                throw new ReglaNegocioException($"«{producto.Descripcion}» es un servicio o adicional y no ingresa a stock.");
            d.Subtotal = d.Cantidad * d.PrecioUnitario;
        }

        compra.Total = compra.Detalles.Sum(d => d.Subtotal);
        compra.Fecha = ServicioOperativo.Ahora();
        compra.Estado = "Registrada";
        compra.Id_Usuarios = idUsuario;
        compra.Comprobante = string.IsNullOrWhiteSpace(compra.Comprobante) ? null : compra.Comprobante.Trim();
        contexto.Compras.Add(compra);
        await contexto.SaveChangesAsync(ct);

        foreach (var d in compra.Detalles)
        {
            var movimiento = await operativo.MoverStockAsync(d.Id_Productos, ReglasNegocio.Entrada, "Compra", d.Cantidad, compra.Id_Compras, compra.Comprobante, idUsuario, ct);
            if (movimiento is null) throw new ReglaNegocioException("Ese producto no maneja stock.");
        }

        await contexto.SaveChangesAsync(ct);
        await transaccion.CommitAsync(ct);
        return compra;
    }

    public async Task AnularAsync(int id, string motivo, int idUsuario, CancellationToken ct = default)
    {
        motivo = ServicioOperativo.ValidarMotivo(motivo);
        var compra = await contexto.Compras.Include(x => x.Detalles).FirstOrDefaultAsync(x => x.Id_Compras == id, ct)
            ?? throw new ReglaNegocioException("Compra no encontrada.");
        await using var transaccion = await contexto.Database.BeginTransactionAsync(ct);
        ServicioOperativo.MarcarAnulada(compra, "Anulada", motivo, idUsuario);
        foreach (var d in compra.Detalles)
            await operativo.MoverStockAsync(d.Id_Productos, ReglasNegocio.Salida, "Anulación compra", d.Cantidad, compra.Id_Compras, motivo, idUsuario, ct, forzar: true);
        await contexto.SaveChangesAsync(ct);
        await transaccion.CommitAsync(ct);
    }
}
