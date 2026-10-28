using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>ABM de roles. Solo usuarios con sesión iniciada pueden administrarlos.</summary>
[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/roles")]
public class RolesController(IRolRepository roles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RolResponse>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok((await roles.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(RolRequest request, CancellationToken cancellationToken)
    {
        var descripcion = request.Descripcion.Trim();
        if (await roles.ExisteDescripcionAsync(descripcion, cancellationToken: cancellationToken))
            return Conflict(new { mensaje = "Ya existe un rol con esa descripción." });

        var rol = new Rol { Descripcion = descripcion };
        await roles.AgregarAsync(rol, cancellationToken);
        await roles.GuardarCambiosAsync(cancellationToken);
        return CreatedAtAction(nameof(ObtenerTodos), new { rol.Id_Roles }, Convertir(rol));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, RolRequest request, CancellationToken cancellationToken)
    {
        var rol = await roles.ObtenerPorIdAsync(id, cancellationToken);
        if (rol is null) return NotFound(new { mensaje = "Rol no encontrado." });

        var descripcion = request.Descripcion.Trim();
        if (await roles.ExisteDescripcionAsync(descripcion, id, cancellationToken))
            return Conflict(new { mensaje = "Ya existe un rol con esa descripción." });

        rol.Descripcion = descripcion;
        await roles.GuardarCambiosAsync(cancellationToken);
        return Ok(Convertir(rol));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var rol = await roles.ObtenerPorIdAsync(id, cancellationToken);
        if (rol is null) return NotFound(new { mensaje = "Rol no encontrado." });
        if (await roles.TieneUsuariosAsync(id, cancellationToken))
            return Conflict(new { mensaje = "No se puede eliminar un rol que tiene usuarios asignados." });

        roles.Eliminar(rol);
        await roles.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private static RolResponse Convertir(Rol rol) => new(rol.Id_Roles, rol.Descripcion);
}
