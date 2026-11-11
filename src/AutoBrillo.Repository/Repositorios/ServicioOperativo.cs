using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

/// <summary>
/// Acciones compartidas por compras, inventario y el consumo de insumos.
/// No guarda cambios: el repositorio que la usa llama a SaveChanges dentro de su transacción.
/// </summary>
public class ServicioOperativo(AutoBrilloDbContext contexto)
{
    public const string Servicio = "Servicio";
    public const string Adicional = "Adicional";
    public const string Insumo = "Insumo";

    /// <summary>Hora local del lavadero, sin zona, para columnas timestamp.</summary>
    public static DateTime Ahora() => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    /// <summary>Servicios y adicionales no mueven stock. Un producto que también es insumo, sí.</summary>
    public static bool ManejaStock(Producto producto) =>
        producto.Tipos.Count == 0 || producto.Tipos.Any(t => !Es(t.Descripcion, Servicio) && !Es(t.Descripcion, Adicional));

    public static bool TieneTipo(Producto producto, string tipo) => producto.Tipos.Any(t => Es(t.Descripcion, tipo));

    public async Task<Producto> ObtenerProductoAsync(int idProducto, CancellationToken ct)
    {
        return await contexto.Productos.Include(x => x.Tipos).FirstOrDefaultAsync(x => x.Id_Productos == idProducto, ct)
            ?? throw new ReglaNegocioException($"El producto {idProducto} no existe.");
    }

    /// <summary>Actualiza StockActual y deja el movimiento en el historial.</summary>
    public async Task<MovimientoStock?> MoverStockAsync(int idProducto, string tipo, string origen, decimal cantidad, int? referencia, string? observacion, int idUsuario, CancellationToken ct, bool forzar = false)
    {
        var producto = await ObtenerProductoAsync(idProducto, ct);
        if (!forzar && !ManejaStock(producto)) return null;
        producto.StockActual = ReglasNegocio.AplicarMovimiento(producto.StockActual, tipo, cantidad);
        var movimiento = new MovimientoStock
        {
            Id_Productos = idProducto,
            Fecha = Ahora(),
            Tipo = tipo,
            Origen = origen.Length > 30 ? origen[..30] : origen,
            Cantidad = cantidad,
            StockResultante = producto.StockActual,
            Id_Referencia = referencia,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
            Id_Usuarios = idUsuario
        };
        if (movimiento.Observacion is { Length: > 250 }) movimiento.Observacion = movimiento.Observacion[..250];
        contexto.MovimientosStock.Add(movimiento);
        return movimiento;
    }

    public static string ValidarMotivo(string? motivo)
    {
        var texto = motivo?.Trim() ?? string.Empty;
        if (texto.Length < 3) throw new ReglaNegocioException("Indicá el motivo de la anulación.");
        return texto.Length > 250 ? texto[..250] : texto;
    }

    public static void MarcarAnulada(OperacionAnulable operacion, string estado, string motivo, int idUsuario)
    {
        if (operacion.Estado == estado) throw new ReglaNegocioException("La operación ya estaba anulada.");
        operacion.Estado = estado;
        operacion.MotivoAnulacion = motivo;
        operacion.FechaAnulacion = Ahora();
        operacion.Id_Usuarios_Anulacion = idUsuario;
    }

    public async Task<bool> PersonaTieneTipoAsync(int idPersona, string tipo, CancellationToken ct) =>
        await contexto.Personas.AnyAsync(p => p.Id_Personas == idPersona && p.Activo && p.Tipos.Any(t => t.Descripcion == tipo), ct);

    public async Task<decimal> ResolverPrecioAsync(int idProducto, int idTipoVehiculo, CancellationToken ct)
    {
        var especifico = await contexto.PreciosServicios
            .Where(x => x.Id_Productos == idProducto && x.Id_Tipos_Vehiculos == idTipoVehiculo)
            .Select(x => (decimal?)x.Precio)
            .FirstOrDefaultAsync(ct);
        if (especifico is not null) return especifico.Value;
        return (await ObtenerProductoAsync(idProducto, ct)).PrecioDeVenta;
    }

    private static bool Es(string valor, string esperado) => string.Equals(valor, esperado, StringComparison.OrdinalIgnoreCase);
}
