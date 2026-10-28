namespace AutoBrillo.Model.Entidades;

/// <summary>Artículo o servicio comercializable, con precios, stock y uno o más tipos.</summary>
public class Producto
{
    public int Id_Productos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioDeCosto { get; set; }
    public decimal PrecioDeVenta { get; set; }
    public bool Activo { get; set; } = true;
    public decimal StockActual { get; set; }
    public List<TipoProducto> Tipos { get; set; } = [];
}
