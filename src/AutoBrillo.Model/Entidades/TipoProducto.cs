namespace AutoBrillo.Model.Entidades;

/// <summary>Clasificación de un producto, por ejemplo Servicio, Producto o Insumo.</summary>
public class TipoProducto
{
    public int Id_Tipos_Productos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public List<Producto> Productos { get; set; } = [];
}
