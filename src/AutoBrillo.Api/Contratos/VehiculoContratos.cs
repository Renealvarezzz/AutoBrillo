using System.ComponentModel.DataAnnotations;

namespace AutoBrillo.Api.Contratos;

public class VehiculoRequest
{
    [Range(1, int.MaxValue)]
    public int Id_Personas { get; set; }

    [Range(1, int.MaxValue)]
    public int Id_Tipos_Vehiculos { get; set; }

    [Required, StringLength(20)]
    public string Patente { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Marca { get; set; }

    [StringLength(80)]
    public string? Modelo { get; set; }

    [StringLength(40)]
    public string? Color { get; set; }

    public bool Activo { get; set; } = true;
}

public record EstadoVehiculoRequest(bool Activo);
