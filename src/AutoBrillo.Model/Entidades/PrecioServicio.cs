namespace AutoBrillo.Model.Entidades;

/// <summary>Precio base de un servicio (o adicional) para un tipo de vehículo. Solo lo modifica el Administrador.</summary>
public class PrecioServicio
{
    public int Id_Precios_Servicios { get; set; }
    public int Id_Productos { get; set; }
    public Producto? Producto { get; set; }
    public int Id_Tipos_Vehiculos { get; set; }
    public TipoVehiculo? TipoVehiculo { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public int Id_Usuarios { get; set; }

    /// <summary>El precio que se usa hoy. Los anteriores quedan en la misma tabla con este indicador en falso.</summary>
    public bool Vigente { get; set; } = true;
}
