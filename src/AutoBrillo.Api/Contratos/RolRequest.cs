using System.ComponentModel.DataAnnotations;

namespace AutoBrillo.Api.Contratos;

/// <summary>Datos necesarios para crear o editar un rol.</summary>
public class RolRequest
{
    [Required(ErrorMessage = "La descripción es obligatoria."), StringLength(100)]
    public string Descripcion { get; set; } = string.Empty;
}
