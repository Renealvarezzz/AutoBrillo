using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public record PrecioLavado(decimal PrecioBase, decimal TotalAdicionales, decimal Total, List<decimal> PreciosAdicionales);

public interface IRepositorioServicios
{
    Task<List<ServicioInsumo>> ObtenerInsumosAsync(int? idServicio, CancellationToken ct = default);
    Task<ServicioInsumo> GuardarInsumoAsync(int idServicio, int idInsumo, decimal cantidad, CancellationToken ct = default);
    Task EliminarInsumoAsync(int id, CancellationToken ct = default);
    Task<PrecioLavado> CalcularPrecioAsync(int idServicio, int idTipoVehiculo, IReadOnlyCollection<int> idsAdicionales, CancellationToken ct = default);
    Task<List<MovimientoStock>> AplicarConsumoAsync(int idServicio, int idUsuario, CancellationToken ct = default);
}

/// <summary>PBI-010 y PBI-005: precio del lavado (RN-008) y consumo de insumos del servicio (RN-014).</summary>
public class RepositorioServicios(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IRepositorioServicios
{
    public Task<List<ServicioInsumo>> ObtenerInsumosAsync(int? idServicio, CancellationToken ct = default) =>
        contexto.ServiciosInsumos.AsNoTracking().Include(x => x.Servicio).Include(x => x.Insumo)
            .Where(x => idServicio == null || x.Id_Servicios == idServicio)
            .OrderBy(x => x.Servicio!.Descripcion).ThenBy(x => x.Insumo!.Descripcion).ToListAsync(ct);

    public async Task<ServicioInsumo> GuardarInsumoAsync(int idServicio, int idInsumo, decimal cantidad, CancellationToken ct = default)
    {
        if (idServicio == idInsumo) throw new ReglaNegocioException("Un servicio no puede consumirse a sí mismo como insumo.");
        var servicio = await operativo.ObtenerProductoAsync(idServicio, ct);
        var insumo = await operativo.ObtenerProductoAsync(idInsumo, ct);
        if (!ServicioOperativo.TieneTipo(servicio, ServicioOperativo.Servicio) && !ServicioOperativo.TieneTipo(servicio, ServicioOperativo.Adicional))
            throw new ReglaNegocioException("Elegí un servicio o un adicional.");
        if (!ServicioOperativo.TieneTipo(insumo, ServicioOperativo.Insumo))
            throw new ReglaNegocioException("Elegí un producto de tipo Insumo.");
        if (cantidad <= 0 || cantidad != decimal.Truncate(cantidad))
            throw new ReglaNegocioException("La cantidad debe ser un entero mayor a cero.");

        var actual = await contexto.ServiciosInsumos.FirstOrDefaultAsync(x => x.Id_Servicios == idServicio && x.Id_Insumos == idInsumo, ct);
        if (actual is null)
        {
            actual = new ServicioInsumo { Id_Servicios = idServicio, Id_Insumos = idInsumo };
            contexto.ServiciosInsumos.Add(actual);
        }

        actual.Cantidad = cantidad;
        await contexto.SaveChangesAsync(ct);
        return actual;
    }

    public async Task EliminarInsumoAsync(int id, CancellationToken ct = default)
    {
        var fila = await contexto.ServiciosInsumos.FindAsync([id], ct) ?? throw new ReglaNegocioException("La configuración de insumo no existe.");
        contexto.ServiciosInsumos.Remove(fila);
        await contexto.SaveChangesAsync(ct);
    }

    public async Task<PrecioLavado> CalcularPrecioAsync(int idServicio, int idTipoVehiculo, IReadOnlyCollection<int> idsAdicionales, CancellationToken ct = default)
    {
        var servicio = await operativo.ObtenerProductoAsync(idServicio, ct);
        if (!ServicioOperativo.TieneTipo(servicio, ServicioOperativo.Servicio))
            throw new ReglaNegocioException("Seleccioná un servicio.");
        if (!await contexto.TiposVehiculos.AnyAsync(x => x.Id_Tipos_Vehiculos == idTipoVehiculo && x.Activo, ct))
            throw new ReglaNegocioException("Seleccioná un tipo de vehículo activo.");

        var precioBase = await operativo.ResolverPrecioAsync(idServicio, idTipoVehiculo, ct);
        var preciosAdicionales = new List<decimal>();
        foreach (var idAdicional in idsAdicionales.Where(x => x > 0).Distinct())
        {
            var adicional = await operativo.ObtenerProductoAsync(idAdicional, ct);
            if (!ServicioOperativo.TieneTipo(adicional, ServicioOperativo.Adicional))
                throw new ReglaNegocioException($"«{adicional.Descripcion}» no es un adicional.");
            preciosAdicionales.Add(await operativo.ResolverPrecioAsync(idAdicional, idTipoVehiculo, ct));
        }

        var total = ReglasNegocio.CalcularPrecioLavado(precioBase, preciosAdicionales);
        return new PrecioLavado(precioBase, preciosAdicionales.Sum(), total, preciosAdicionales);
    }

    public async Task<List<MovimientoStock>> AplicarConsumoAsync(int idServicio, int idUsuario, CancellationToken ct = default)
    {
        var servicio = await operativo.ObtenerProductoAsync(idServicio, ct);
        if (!ServicioOperativo.TieneTipo(servicio, ServicioOperativo.Servicio) && !ServicioOperativo.TieneTipo(servicio, ServicioOperativo.Adicional))
            throw new ReglaNegocioException("Seleccioná un servicio o un adicional.");

        var configurados = await contexto.ServiciosInsumos.AsNoTracking().Where(x => x.Id_Servicios == idServicio).ToListAsync(ct);
        var consumo = ReglasNegocio.ConsolidarConsumo(configurados.Select(x => (x.Id_Insumos, x.Cantidad)));
        if (consumo.Count == 0) throw new ReglaNegocioException("Este servicio no tiene insumos configurados.");

        await using var transaccion = await contexto.Database.BeginTransactionAsync(ct);
        var movimientos = new List<MovimientoStock>();
        foreach (var (idInsumo, cantidad) in consumo)
        {
            var movimiento = await operativo.MoverStockAsync(idInsumo, ReglasNegocio.Salida, "Consumo servicio", cantidad, idServicio, servicio.Descripcion, idUsuario, ct, forzar: true);
            if (movimiento is not null) movimientos.Add(movimiento);
        }

        await contexto.SaveChangesAsync(ct);
        await transaccion.CommitAsync(ct);
        return movimientos;
    }
}
