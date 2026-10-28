using System.ComponentModel.DataAnnotations;

namespace AutoBrillo.Api.Contratos;

/// <summary>Datos para crear o editar un usuario desde el ABM.</summary>
public class UsuarioRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Contraseña nueva. Es obligatoria al crear y opcional al editar.</summary>
    public string? Password { get; set; }

    /// <summary>Uno o varios roles obligatorios para el usuario.</summary>
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos un rol.")]
    public List<int> Id_Roles { get; set; } = [];
}
