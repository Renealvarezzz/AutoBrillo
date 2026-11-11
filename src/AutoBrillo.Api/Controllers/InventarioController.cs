using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>PBI-005: movimientos y saldo de productos de venta e insumos.</summary>
[ApiController]
[Authorize(Roles = RolesAutoBrillo.AdminStock)]
[Route("api/movimientos-stock")]
public class InventarioController(IRepositorioInventario inventario) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int? idProducto, CancellationToken ct) =>
        Ok((await inventario.ObtenerMovimientosAsync(idProducto, ct)).Select(m => new
        {
            m.Id_Movimientos_Stock,
            m.Id_Productos,
            Producto = m.Producto?.Descripcion,
            m.Fecha,
            m.Tipo,
            m.Origen,
            m.Cantidad,
            m.StockResultante,
            m.Id_Referencia,
            m.Observacion,
            m.Id_Usuarios
        }));

    [HttpPost]
    public async Task<IActionResult> Registrar(MovimientoStockRequest request, CancellationToken ct)
    {
        var origen = string.IsNullOrWhiteSpace(request.Origen) ? (request.Tipo == "Entrada" ? "Ajuste" : "Consumo interno") : request.Origen.Trim();
        var m = await inventario.RegistrarMovimientoAsync(request.Id_Productos, request.Tipo, origen, request.Cantidad, request.Observacion?.Trim(), User.IdUsuario(), ct);
        return Ok(new { m.Id_Movimientos_Stock, m.StockResultante });
    }
}
