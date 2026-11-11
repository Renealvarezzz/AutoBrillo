namespace AutoBrillo.Model.Entidades;

/// <summary>Catálogo de tipos de vehículo usado por los precios y por los vehículos del cliente.</summary>
public class TipoVehiculo
{
    public int Id_Tipos_Vehiculos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
