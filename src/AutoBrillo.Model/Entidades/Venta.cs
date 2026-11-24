namespace AutoBrillo.Model.Entidades;

public class Venta : IOperacionAnulable
{
    public int Id_Ventas { get; set; }
    public int? Id_Lavados { get; set; }
    public Lavado? Lavado { get; set; }
    public DateTime Fecha { get; set; }
    public string Condicion { get; set; } = "Contado";
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Registrada";
    public int Id_Usuarios { get; set; }
    public Usuario? Usuario { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public int? Id_Usuarios_Anulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public List<VentaDetalle> Detalles { get; set; } = [];
    public List<Cobro> Cobros { get; set; } = [];
    public List<Comision> Comisiones { get; set; } = [];
}

public class VentaDetalle
{
    public int Id_Ventas_Detalle { get; set; }
    public int Id_Ventas { get; set; }
    public int Id_Productos { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class Cobro
{
    public int Id_Cobros { get; set; }
    public int Id_Ventas { get; set; }
    public DateTime Fecha { get; set; }
    public string MedioPago { get; set; } = "Efectivo";
    public decimal Importe { get; set; }
    public int Id_Usuarios { get; set; }
}

public class ConfiguracionComision
{
    public int Id_Configuracion_Comision { get; set; }
    public decimal Porcentaje { get; set; } = 30;
    public DateTime FechaActualizacion { get; set; }
    public int Id_Usuarios { get; set; }
}

public class Comision
{
    public int Id_Comisiones { get; set; }
    public int Id_Ventas { get; set; }
    public Venta? Venta { get; set; }
    public int Id_Personas { get; set; }
    public Persona? Persona { get; set; }
    public decimal PorcentajeAplicado { get; set; }
    public decimal Importe { get; set; }
    public bool Anulada { get; set; }
}
