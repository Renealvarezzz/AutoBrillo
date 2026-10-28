using System.Security.Claims;
using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>ABM de usuarios con uno o varios roles. Las contraseñas se guardan mediante BCrypt.</summary>
[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/usuarios")]
public class UsuariosController(IUserRepository usuarios, IRolRepository roles, PasswordHasher passwordHasher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok((await usuarios.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { mensaje = "La contraseña es obligatoria al crear un usuario." });
        if (request.Password.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });
        var nombre = request.Nombre.Trim();
        if (await usuarios.ExisteNombreAsync(nombre, cancellationToken))
            return Conflict(new { mensaje = "El nombre de usuario ya está registrado." });

        var rolesSeleccionados = await ObtenerRolesValidosAsync(request.Id_Roles, cancellationToken);
        if (rolesSeleccionados is null) return BadRequest(new { mensaje = "Debe seleccionar al menos un rol válido." });

        var usuario = new Usuario { Nombre = nombre, Password = passwordHasher.Hashear(request.Password), Roles = rolesSeleccionados };
        await usuarios.AgregarAsync(usuario, cancellationToken);
        await usuarios.GuardarCambiosAsync(cancellationToken);
        usuario = await usuarios.ObtenerPorIdAsync(usuario.Id_Usuarios, cancellationToken) ?? usuario;
        return CreatedAtAction(nameof(ObtenerTodos), new { usuario.Id_Usuarios }, Convertir(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, UsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });
        var nombre = request.Nombre.Trim();
        if (await usuarios.ExisteNombreAsync(nombre, cancellationToken) && usuario.Nombre != nombre)
            return Conflict(new { mensaje = "El nombre de usuario ya está registrado." });

        var rolesSeleccionados = await ObtenerRolesValidosAsync(request.Id_Roles, cancellationToken);
        if (rolesSeleccionados is null) return BadRequest(new { mensaje = "Debe seleccionar al menos un rol válido." });

        if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

        usuario.Nombre = nombre;
        usuario.Roles = rolesSeleccionados;
        if (!string.IsNullOrWhiteSpace(request.Password)) usuario.Password = passwordHasher.Hashear(request.Password);
        await usuarios.GuardarCambiosAsync(cancellationToken);
        usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken) ?? usuario;
        return Ok(Convertir(usuario));
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });

        var idAdministradorActual = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!request.Activo && idAdministradorActual == id.ToString())
            return BadRequest(new { mensaje = "No podés desactivar tu propio usuario." });

        usuario.Activo = request.Activo;
        await usuarios.GuardarCambiosAsync(cancellationToken);
        usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken) ?? usuario;
        return Ok(Convertir(usuario));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(id, cancellationToken);
        if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });
        usuarios.Eliminar(usuario);
        await usuarios.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private async Task<List<Rol>?> ObtenerRolesValidosAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var idsUnicos = ids.Distinct().ToList();
        if (idsUnicos.Count == 0) return null;
        var resultado = new List<Rol>();
        foreach (var id in idsUnicos)
        {
            var rol = await roles.ObtenerPorIdAsync(id, cancellationToken);
            if (rol is null) return null;
            resultado.Add(rol);
        }
        return resultado;
    }

    private static UsuarioResponse Convertir(Usuario usuario) => new(
        usuario.Id_Usuarios,
        usuario.Nombre,
        usuario.Roles.Select(x => x.Id_Roles).ToList(),
        usuario.Roles.OrderBy(x => x.Descripcion).Select(x => x.Descripcion).ToList(),
        usuario.Activo);
}
