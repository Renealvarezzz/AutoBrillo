using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public interface IRepositorioInventario
{
    Task<List<MovimientoStock>> ObtenerMovimientosAsync(int? idProducto, CancellationToken ct = default);
    Task<MovimientoStock> RegistrarMovimientoAsync(int idProducto, string tipo, string origen, decimal cantidad, string? observacion, int idUsuario, CancellationToken ct = default);
}

/// <summary>PBI-005: historial de stock y ajustes manuales.</summary>
public class RepositorioInventario(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IRepositorioInventario
{
    public Task<List<MovimientoStock>> ObtenerMovimientosAsync(int? idProducto, CancellationToken ct = default) =>
        contexto.MovimientosStock.AsNoTracking().Include(x => x.Producto)
            .Where(x => idProducto == null || x.Id_Productos == idProducto)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id_Movimientos_Stock).Take(500).ToListAsync(ct);

    public async Task<MovimientoStock> RegistrarMovimientoAsync(int idProducto, string tipo, string origen, decimal cantidad, string? observacion, int idUsuario, CancellationToken ct = default)
    {
        var movimiento = await operativo.MoverStockAsync(idProducto, tipo, origen, cantidad, null, observacion, idUsuario, ct, forzar: true)
            ?? throw new ReglaNegocioException("No se pudo registrar el movimiento.");
        await contexto.SaveChangesAsync(ct);
        return movimiento;
    }
}
