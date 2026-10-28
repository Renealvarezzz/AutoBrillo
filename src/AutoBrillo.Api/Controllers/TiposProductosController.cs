using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/tipos-productos")]
public class TiposProductosController(ITipoProductoRepository tipos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TipoProductoResponse>>> ObtenerTodos(CancellationToken cancellationToken) => Ok((await tipos.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(TipoProductoRequest request, CancellationToken cancellationToken)
    {
        var descripcion = request.Descripcion.Trim();
        if (string.IsNullOrWhiteSpace(descripcion)) return BadRequest(new { mensaje = "La descripción es obligatoria." });
        if (await tipos.ExisteDescripcionAsync(descripcion, cancellationToken: cancellationToken)) return Conflict(new { mensaje = "Ya existe ese tipo de producto." });
        var tipo = new TipoProducto { Descripcion = descripcion };
        await tipos.AgregarAsync(tipo, cancellationToken);
        await tipos.GuardarCambiosAsync(cancellationToken);
        return CreatedAtAction(nameof(ObtenerTodos), new { tipo.Id_Tipos_Productos }, Convertir(tipo));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, TipoProductoRequest request, CancellationToken cancellationToken)
    {
        var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
        if (tipo is null) return NotFound(new { mensaje = "Tipo de producto no encontrado." });
        var descripcion = request.Descripcion.Trim();
        if (string.IsNullOrWhiteSpace(descripcion)) return BadRequest(new { mensaje = "La descripción es obligatoria." });
        if (await tipos.ExisteDescripcionAsync(descripcion, id, cancellationToken)) return Conflict(new { mensaje = "Ya existe ese tipo de producto." });
        tipo.Descripcion = descripcion;
        await tipos.GuardarCambiosAsync(cancellationToken);
        return Ok(Convertir(tipo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
        if (tipo is null) return NotFound(new { mensaje = "Tipo de producto no encontrado." });
        if (await tipos.TieneProductosAsync(id, cancellationToken)) return Conflict(new { mensaje = "No se puede eliminar un tipo que está asignado a productos." });
        tipos.Eliminar(tipo);
        await tipos.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private static TipoProductoResponse Convertir(TipoProducto tipo) => new(tipo.Id_Tipos_Productos, tipo.Descripcion);
}
