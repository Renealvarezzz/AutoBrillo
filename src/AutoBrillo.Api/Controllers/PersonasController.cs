using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/personas")]
public class PersonasController(IPersonaRepository personas, ITipoPersonaRepository tipos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PersonaResponse>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok((await personas.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(PersonaRequest request, CancellationToken cancellationToken)
    {
        var validacion = await ValidarAsync(request, null, cancellationToken);
        if (validacion.Error is not null) return validacion.Error;
        var persona = new Persona { Nombre = request.Nombre.Trim(), Documento = request.Documento.Trim(), Telefono = LimpiarTelefono(request.Telefono), Tipos = validacion.Tipos! };
        await personas.AgregarAsync(persona, cancellationToken);
        await personas.GuardarCambiosAsync(cancellationToken);
        persona = await personas.ObtenerPorIdAsync(persona.Id_Personas, cancellationToken) ?? persona;
        return CreatedAtAction(nameof(ObtenerTodos), new { persona.Id_Personas }, Convertir(persona));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, PersonaRequest request, CancellationToken cancellationToken)
    {
        var persona = await personas.ObtenerPorIdAsync(id, cancellationToken);
        if (persona is null) return NotFound(new { mensaje = "Persona no encontrada." });
        var validacion = await ValidarAsync(request, id, cancellationToken);
        if (validacion.Error is not null) return validacion.Error;
        persona.Nombre = request.Nombre.Trim();
        persona.Documento = request.Documento.Trim();
        persona.Telefono = LimpiarTelefono(request.Telefono);
        persona.Tipos = validacion.Tipos!;
        await personas.GuardarCambiosAsync(cancellationToken);
        persona = await personas.ObtenerPorIdAsync(id, cancellationToken) ?? persona;
        return Ok(Convertir(persona));
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoUsuarioRequest request, CancellationToken cancellationToken)
    {
        var persona = await personas.ObtenerPorIdAsync(id, cancellationToken);
        if (persona is null) return NotFound(new { mensaje = "Persona no encontrada." });
        persona.Activo = request.Activo;
        await personas.GuardarCambiosAsync(cancellationToken);
        return Ok(Convertir(persona));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var persona = await personas.ObtenerPorIdAsync(id, cancellationToken);
        if (persona is null) return NotFound(new { mensaje = "Persona no encontrada." });
        personas.Eliminar(persona);
        await personas.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private async Task<(IActionResult? Error, List<TipoPersona>? Tipos)> ValidarAsync(PersonaRequest request, int? excluirId, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre?.Trim() ?? string.Empty;
        var documento = request.Documento?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(documento)) return (BadRequest(new { mensaje = "Nombre y documento son obligatorios." }), null);
        if (await personas.ExisteDocumentoAsync(documento, excluirId, cancellationToken)) return (Conflict(new { mensaje = "Ya existe una persona con ese documento." }), null);
        var ids = request.Id_Tipos_Personas.Distinct().ToList();
        if (ids.Count == 0) return (BadRequest(new { mensaje = "Seleccioná al menos un tipo de persona." }), null);
        var seleccionados = new List<TipoPersona>();
        foreach (var id in ids)
        {
            var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
            if (tipo is null) return (BadRequest(new { mensaje = "Hay un tipo de persona inválido." }), null);
            seleccionados.Add(tipo);
        }
        return (null, seleccionados);
    }

    private static string? LimpiarTelefono(string? telefono) => string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    private static PersonaResponse Convertir(Persona persona) => new(persona.Id_Personas, persona.Nombre, persona.Documento, persona.Telefono, persona.Activo, persona.Tipos.Select(x => x.Id_Tipos_Personas).ToList(), persona.Tipos.OrderBy(x => x.Descripcion).Select(x => x.Descripcion).ToList());
}
