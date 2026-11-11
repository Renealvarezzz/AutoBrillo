using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>
/// PBI-010 y PBI-005: precios por tipo de vehículo (solo Administrador),
/// adicionales, insumos del servicio, precio del lavado (RN-008) y consumo (RN-014).
/// </summary>
[ApiController]
[Authorize(Roles = RolesAutoBrillo.Administrador)]
[Route("api")]
public class ServiciosController(IRepositorioCatalogos catalogos, IRepositorioServicios servicios) : ControllerBase
{
    [HttpGet("precios-servicios")]
    public async Task<IActionResult> Precios(CancellationToken ct) => Ok((await catalogos.ObtenerPreciosAsync(ct)).Select(p => new
    {
        p.Id_Precios_Servicios,
        p.Id_Productos,
        Producto = p.Producto?.Descripcion,
        p.Id_Tipos_Vehiculos,
        TipoVehiculo = p.TipoVehiculo?.Descripcion,
        p.Precio,
        p.FechaActualizacion
    }));

    [HttpPut("precios-servicios")]
    public async Task<IActionResult> GuardarPrecio(PrecioServicioRequest request, CancellationToken ct)
    {
        if (!User.IsInRole(RolesAutoBrillo.Administrador)) return Forbid();
        var p = await catalogos.GuardarPrecioAsync(request.Id_Productos, request.Id_Tipos_Vehiculos, request.Precio, User.IdUsuario(), ct);
        return Ok(new { p.Id_Precios_Servicios, p.Precio });
    }

    [HttpDelete("precios-servicios/{id:int}")]
    public async Task<IActionResult> EliminarPrecio(int id, CancellationToken ct)
    {
        if (!User.IsInRole(RolesAutoBrillo.Administrador)) return Forbid();
        await catalogos.EliminarPrecioAsync(id, ct);
        return NoContent();
    }

    [HttpGet("servicios/precio")]
    public async Task<IActionResult> Precio([FromQuery] int idServicio, [FromQuery] int idTipoVehiculo, [FromQuery] int[]? adicionales, CancellationToken ct)
    {
        var precio = await servicios.CalcularPrecioAsync(idServicio, idTipoVehiculo, adicionales ?? [], ct);
        return Ok(new { precio.PrecioBase, precio.TotalAdicionales, precio.Total });
    }

    [HttpGet("servicios/insumos")]
    public async Task<IActionResult> Insumos([FromQuery] int? idServicio, CancellationToken ct) =>
        Ok((await servicios.ObtenerInsumosAsync(idServicio, ct)).Select(x => new
        {
            x.Id_Servicios_Insumos,
            x.Id_Servicios,
            Servicio = x.Servicio?.Descripcion,
            x.Id_Insumos,
            Insumo = x.Insumo?.Descripcion,
            x.Cantidad
        }));

    [HttpPut("servicios/insumos")]
    public async Task<IActionResult> GuardarInsumo(ServicioInsumoRequest request, CancellationToken ct)
    {
        var fila = await servicios.GuardarInsumoAsync(request.Id_Servicios, request.Id_Insumos, request.Cantidad, ct);
        return Ok(new { fila.Id_Servicios_Insumos, fila.Cantidad });
    }

    [HttpDelete("servicios/insumos/{id:int}")]
    public async Task<IActionResult> EliminarInsumo(int id, CancellationToken ct)
    {
        await servicios.EliminarInsumoAsync(id, ct);
        return NoContent();
    }

    /// <summary>RN-014: descuenta del stock los insumos configurados para el servicio.</summary>
    [HttpPost("servicios/{id:int}/consumo")]
    public async Task<IActionResult> Consumir(int id, CancellationToken ct)
    {
        var movimientos = await servicios.AplicarConsumoAsync(id, User.IdUsuario(), ct);
        return Ok(movimientos.Select(m => new { m.Id_Productos, m.Cantidad, m.StockResultante }));
    }
}
