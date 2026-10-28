using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/tipos-personas")]
public class TiposPersonasController(ITipoPersonaRepository tipos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TipoPersonaResponse>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok((await tipos.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(TipoPersonaRequest request, CancellationToken cancellationToken)
    {
        var descripcion = request.Descripcion.Trim();
        if (string.IsNullOrWhiteSpace(descripcion)) return BadRequest(new { mensaje = "La descripción es obligatoria." });
        if (await tipos.ExisteDescripcionAsync(descripcion, cancellationToken: cancellationToken)) return Conflict(new { mensaje = "Ya existe ese tipo de persona." });
        var tipo = new TipoPersona { Descripcion = descripcion };
        await tipos.AgregarAsync(tipo, cancellationToken);
        await tipos.GuardarCambiosAsync(cancellationToken);
        return CreatedAtAction(nameof(ObtenerTodos), new { tipo.Id_Tipos_Personas }, Convertir(tipo));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, TipoPersonaRequest request, CancellationToken cancellationToken)
    {
        var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
        if (tipo is null) return NotFound(new { mensaje = "Tipo de persona no encontrado." });
        var descripcion = request.Descripcion.Trim();
        if (string.IsNullOrWhiteSpace(descripcion)) return BadRequest(new { mensaje = "La descripción es obligatoria." });
        if (await tipos.ExisteDescripcionAsync(descripcion, id, cancellationToken)) return Conflict(new { mensaje = "Ya existe ese tipo de persona." });
        tipo.Descripcion = descripcion;
        await tipos.GuardarCambiosAsync(cancellationToken);
        return Ok(Convertir(tipo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
        if (tipo is null) return NotFound(new { mensaje = "Tipo de persona no encontrado." });
        if (await tipos.TienePersonasAsync(id, cancellationToken)) return Conflict(new { mensaje = "No se puede eliminar un tipo que está asignado a personas." });
        tipos.Eliminar(tipo);
        await tipos.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private static TipoPersonaResponse Convertir(TipoPersona tipo) => new(tipo.Id_Tipos_Personas, tipo.Descripcion);
}
