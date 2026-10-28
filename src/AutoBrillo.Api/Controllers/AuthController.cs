using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AutoBrillo.Api.Controllers;

/// <summary>
/// Atiende las acciones relacionadas con el acceso de usuarios.
/// Su ruta base es /api/auth. El controlador recibe Repository y PasswordHasher por inyección de dependencias.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository usuarios, IRolRepository roles, PasswordHasher passwordHasher, IConfiguration configuracion) : ControllerBase
{
    /// <summary>
    /// Crea un usuario nuevo.
    /// Ejemplo de solicitud: { "nombre": "ana", "password": "clave123" }.
    /// </summary>
    /// <returns>
    /// 201 con Id_Usuarios y Nombre si se registró; 409 si el nombre ya estaba usado.
    /// </returns>
    [HttpPost("register")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Registro(CredencialesRequest request, CancellationToken cancellationToken)
    {
        // Trim elimina espacios involuntarios al inicio y al final del nombre.
        var nombre = request.Nombre.Trim();

        // Antes de guardar, comprobamos que no exista otro usuario con el mismo Nombre.
        if (await usuarios.ExisteNombreAsync(nombre, cancellationToken))
            return Conflict(new { mensaje = "El nombre de usuario ya está registrado." });

        // PasswordHasher convierte la contraseña a BCrypt. El registro original asigna Administrador por defecto.
        var administrador = await roles.ObtenerPorDescripcionAsync("Administrador", cancellationToken);
        if (administrador is null) return Problem("No existe el rol Administrador.", statusCode: StatusCodes.Status500InternalServerError);
        var usuario = new Usuario { Nombre = nombre, Password = passwordHasher.Hashear(request.Password), Roles = [administrador] };

        // Se agrega el objeto y después se confirma el INSERT en PostgreSQL.
        await usuarios.AgregarAsync(usuario, cancellationToken);
        await usuarios.GuardarCambiosAsync(cancellationToken);

        // Solo devolvemos los datos seguros que la web necesita; nunca Password ni su hash.
        return CreatedAtAction(nameof(Me), new { }, new { usuario.Id_Usuarios, usuario.Nombre });
    }

    /// <summary>
    /// Comprueba las credenciales y devuelve un JWT para mantener la sesión.
    /// Ejemplo de solicitud: { "nombre": "ana", "password": "clave123" }.
    /// </summary>
    /// <returns>200 con token y usuario; 401 cuando el nombre o la contraseña no coinciden.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(CredencialesRequest request, CancellationToken cancellationToken)
    {
        // Busca primero al usuario por Nombre.
        var usuario = await usuarios.ObtenerPorNombreAsync(request.Nombre.Trim(), cancellationToken);

        // Si no existe, está desactivado o BCrypt no valida la contraseña, no se revela cuál de los datos falló.
        if (usuario is null || !usuario.Activo || !passwordHasher.Verificar(request.Password, usuario.Password))
            return Unauthorized(new { mensaje = "Nombre o contraseña incorrectos." });

        // El token permite acceder posteriormente a endpoints marcados con [Authorize].
        return Ok(new { token = CrearToken(usuario), usuario = usuario.Nombre, esAdministrador = usuario.Roles.Any(rol => rol.Descripcion == "Administrador") });
    }

    /// <summary>
    /// Devuelve el identificador y nombre del usuario que envió un JWT válido.
    /// [Authorize] hace que ASP.NET Core rechace automáticamente solicitudes sin token válido.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        usuario = User.Identity?.Name,
        esAdministrador = User.IsInRole("Administrador")
    });

    /// <summary>
    /// Crea un JWT firmado con datos mínimos: identificador y nombre del usuario.
    /// El token dura 8 horas y se firma con JWT_SECRET, que viene de las variables de entorno.
    /// </summary>
    private string CrearToken(Usuario usuario)
    {
        var clave = configuracion["JWT_SECRET"] ?? throw new InvalidOperationException("Falta la variable de entorno JWT_SECRET.");
        var issuer = configuracion["JWT_ISSUER"] ?? "AutoBrillo.Api";
        var audience = configuracion["JWT_AUDIENCE"] ?? "AutoBrillo.Web";

        // HMAC SHA-256 firma el token para que no pueda ser modificado por el navegador.
        var credenciales = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)), SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id_Usuarios.ToString()),
            new(ClaimTypes.Name, usuario.Nombre)
        };
        claims.AddRange(usuario.Roles.Select(rol => new Claim(ClaimTypes.Role, rol.Descripcion)));

        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credenciales);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
