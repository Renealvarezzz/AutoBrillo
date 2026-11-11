namespace AutoBrillo.Model.Entidades;

/// <summary>Cada entrada o salida de stock queda registrada con su origen y responsable.</summary>
public class MovimientoStock
{
    public int Id_Movimientos_Stock { get; set; }
    public int Id_Productos { get; set; }
    public Producto? Producto { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal StockResultante { get; set; }
    public int? Id_Referencia { get; set; }
    public string? Observacion { get; set; }
    public int Id_Usuarios { get; set; }
}
