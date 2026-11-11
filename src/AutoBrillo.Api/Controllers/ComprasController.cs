using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>PBI-006: compras a proveedores. Registrarlas puede Stock; anularlas, solo el Administrador.</summary>
[ApiController]
[Authorize(Roles = RolesAutoBrillo.AdminStock)]
[Route("api/compras")]
public class ComprasController(IRepositorioCompras compras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken ct) => Ok((await compras.ObtenerTodasAsync(ct)).Select(c => new
    {
        c.Id_Compras,
        c.Id_Personas,
        Proveedor = c.Proveedor?.Nombre,
        c.Fecha,
        c.Comprobante,
        c.Total,
        c.Estado,
        c.MotivoAnulacion,
        c.FechaAnulacion,
        Detalles = c.Detalles.Select(d => new { d.Id_Productos, Producto = d.Producto?.Descripcion, d.Cantidad, d.PrecioUnitario, d.Subtotal })
    }));

    [HttpPost]
    public async Task<IActionResult> Registrar(CompraRequest request, CancellationToken ct)
    {
        var compra = new Compra
        {
            Id_Personas = request.Id_Personas,
            Comprobante = request.Comprobante?.Trim(),
            Detalles = (request.Detalles ?? []).Select(d => new CompraDetalle { Id_Productos = d.Id_Productos, Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario }).ToList()
        };
        compra = await compras.RegistrarAsync(compra, User.IdUsuario(), ct);
        return Ok(new { compra.Id_Compras, compra.Total });
    }

    [HttpPost("{id:int}/anular")]
    [Authorize(Roles = RolesAutoBrillo.Administrador)]
    public async Task<IActionResult> Anular(int id, AnulacionRequest request, CancellationToken ct)
    {
        if (!User.IsInRole(RolesAutoBrillo.Administrador)) return Forbid();
        await compras.AnularAsync(id, request.Motivo ?? string.Empty, User.IdUsuario(), ct);
        return NoContent();
    }
}
