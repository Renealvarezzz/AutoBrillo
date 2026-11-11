namespace AutoBrillo.Model.Entidades;

/// <summary>Cantidad de un insumo que consume un servicio (PBI-005, RN-014).</summary>
public class ServicioInsumo
{
    public int Id_Servicios_Insumos { get; set; }
    public int Id_Servicios { get; set; }
    public Producto? Servicio { get; set; }
    public int Id_Insumos { get; set; }
    public Producto? Insumo { get; set; }
    public decimal Cantidad { get; set; }
}
