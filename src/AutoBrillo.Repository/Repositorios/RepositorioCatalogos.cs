using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public interface IRepositorioCatalogos
{
    Task<List<TipoVehiculo>> ObtenerTiposVehiculosAsync(CancellationToken ct = default);
    Task<TipoVehiculo> GuardarTipoVehiculoAsync(int id, string descripcion, bool activo, CancellationToken ct = default);
    Task<List<PrecioServicio>> ObtenerPreciosAsync(CancellationToken ct = default);
    Task<PrecioServicio> GuardarPrecioAsync(int idProducto, int idTipoVehiculo, decimal precio, int idUsuario, CancellationToken ct = default);
    Task EliminarPrecioAsync(int id, CancellationToken ct = default);
}

/// <summary>PBI-010: tipos de vehículo y precio base por tipo. Los precios los cambia el Administrador.</summary>
public class RepositorioCatalogos(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IRepositorioCatalogos
{
    public Task<List<TipoVehiculo>> ObtenerTiposVehiculosAsync(CancellationToken ct = default) =>
        contexto.TiposVehiculos.AsNoTracking().OrderBy(x => x.Id_Tipos_Vehiculos).ToListAsync(ct);

    public async Task<TipoVehiculo> GuardarTipoVehiculoAsync(int id, string descripcion, bool activo, CancellationToken ct = default)
    {
        descripcion = Validar(descripcion);
        if (await contexto.TiposVehiculos.AnyAsync(x => x.Descripcion == descripcion && x.Id_Tipos_Vehiculos != id, ct))
            throw new ReglaNegocioException("Ya existe ese tipo de vehículo.");
        var tipo = id == 0
            ? contexto.TiposVehiculos.Add(new TipoVehiculo()).Entity
            : await contexto.TiposVehiculos.FindAsync([id], ct) ?? throw new ReglaNegocioException("Tipo de vehículo no encontrado.");
        tipo.Descripcion = descripcion;
        tipo.Activo = activo;
        await contexto.SaveChangesAsync(ct);
        return tipo;
    }

    public Task<List<PrecioServicio>> ObtenerPreciosAsync(CancellationToken ct = default) =>
        contexto.PreciosServicios.AsNoTracking().Include(x => x.Producto).Include(x => x.TipoVehiculo)
            .OrderBy(x => x.Producto!.Descripcion).ThenBy(x => x.Id_Tipos_Vehiculos).ToListAsync(ct);

    public async Task<PrecioServicio> GuardarPrecioAsync(int idProducto, int idTipoVehiculo, decimal precio, int idUsuario, CancellationToken ct = default)
    {
        if (precio < 0 || precio != decimal.Truncate(precio)) throw new ReglaNegocioException("El precio debe ser un entero no negativo.");
        var producto = await operativo.ObtenerProductoAsync(idProducto, ct);
        if (!ServicioOperativo.TieneTipo(producto, ServicioOperativo.Servicio) && !ServicioOperativo.TieneTipo(producto, ServicioOperativo.Adicional))
            throw new ReglaNegocioException("El precio por tipo de vehículo se define sobre un servicio o un adicional.");
        if (!await contexto.TiposVehiculos.AnyAsync(x => x.Id_Tipos_Vehiculos == idTipoVehiculo, ct))
            throw new ReglaNegocioException("Tipo de vehículo inválido.");

        var actual = await contexto.PreciosServicios.FirstOrDefaultAsync(x => x.Id_Productos == idProducto && x.Id_Tipos_Vehiculos == idTipoVehiculo, ct);
        if (actual is null)
        {
            actual = new PrecioServicio { Id_Productos = idProducto, Id_Tipos_Vehiculos = idTipoVehiculo };
            contexto.PreciosServicios.Add(actual);
        }

        actual.Precio = precio;
        actual.FechaActualizacion = ServicioOperativo.Ahora();
        actual.Id_Usuarios = idUsuario;
        await contexto.SaveChangesAsync(ct);
        return actual;
    }

    public async Task EliminarPrecioAsync(int id, CancellationToken ct = default)
    {
        var precio = await contexto.PreciosServicios.FindAsync([id], ct) ?? throw new ReglaNegocioException("Precio no encontrado.");
        contexto.PreciosServicios.Remove(precio);
        await contexto.SaveChangesAsync(ct);
    }

    private static string Validar(string descripcion)
    {
        var texto = descripcion?.Trim() ?? string.Empty;
        if (texto.Length == 0) throw new ReglaNegocioException("La descripción es obligatoria.");
        return texto.Length > 100 ? texto[..100] : texto;
    }
}
