namespace AutoBrillo.Model.Entidades;

/// <summary>Vehículo de un cliente (PBI-004).</summary>
public class Vehiculo
{
    public int Id_Vehiculos { get; set; }
    public int Id_Personas { get; set; }
    public Persona? Cliente { get; set; }
    public int Id_Tipos_Vehiculos { get; set; }
    public TipoVehiculo? TipoVehiculo { get; set; }
    public string Patente { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; } = true;
}
