namespace AutoBrillo.Model.Entidades;

/// <summary>Datos comunes de una operación que el Administrador puede anular.</summary>
public abstract class OperacionAnulable
{
    public string Estado { get; set; } = string.Empty;
    public int Id_Usuarios { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public int? Id_Usuarios_Anulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
}

/// <summary>Compra a un proveedor. Cada línea suma stock (PBI-006).</summary>
public class Compra : OperacionAnulable
{
    public int Id_Compras { get; set; }
    public int Id_Personas { get; set; }
    public Persona? Proveedor { get; set; }
    public DateTime Fecha { get; set; }
    public string? Comprobante { get; set; }
    public decimal Total { get; set; }
    public List<CompraDetalle> Detalles { get; set; } = [];
}

public class CompraDetalle
{
    public int Id_Compras_Detalle { get; set; }
    public int Id_Compras { get; set; }
    public int Id_Productos { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
