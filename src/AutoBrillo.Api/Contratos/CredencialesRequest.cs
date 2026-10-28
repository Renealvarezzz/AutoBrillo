using System.ComponentModel.DataAnnotations;

namespace AutoBrillo.Api.Contratos;

/// <summary>
/// Datos que la web envía a la API para registrar o iniciar sesión.
/// El mismo formato se usa en POST /api/auth/register y POST /api/auth/login.
/// </summary>
public class CredencialesRequest
{
    /// <summary>Nombre de usuario. Es obligatorio y admite hasta 100 caracteres.</summary>
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña escrita por el usuario. Es obligatoria y debe tener al menos 6 caracteres.
    /// La API la recibe aquí, pero nunca la guarda directamente: Repository la transforma con BCrypt.
    /// </summary>
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
