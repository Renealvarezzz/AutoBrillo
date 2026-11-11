namespace AutoBrillo.Web.Servicios;

public class TipoVehiculoDto
{
    public int Id_Tipos_Vehiculos { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class VehiculoDto
{
    public int Id_Vehiculos { get; set; }
    public int Id_Personas { get; set; }
    public string? Cliente { get; set; }
    public int Id_Tipos_Vehiculos { get; set; }
    public string? TipoVehiculo { get; set; }
    public string Patente { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; } = true;
}

public class PrecioServicioDto
{
    public int Id_Precios_Servicios { get; set; }
    public int Id_Productos { get; set; }
    public string? Producto { get; set; }
    public int Id_Tipos_Vehiculos { get; set; }
    public string? TipoVehiculo { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class ServicioInsumoDto
{
    public int Id_Servicios_Insumos { get; set; }
    public int Id_Servicios { get; set; }
    public string? Servicio { get; set; }
    public int Id_Insumos { get; set; }
    public string? Insumo { get; set; }
    public decimal Cantidad { get; set; }
}

public class PrecioLavadoDto
{
    public decimal PrecioBase { get; set; }
    public decimal TotalAdicionales { get; set; }
    public decimal Total { get; set; }
}

public class MovimientoStockDto
{
    public int Id_Movimientos_Stock { get; set; }
    public int Id_Productos { get; set; }
    public string? Producto { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = "";
    public string Origen { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal StockResultante { get; set; }
    public int? Id_Referencia { get; set; }
    public string? Observacion { get; set; }
}

public class LineaDto
{
    public int Id_Productos { get; set; }
    public string? Producto { get; set; }
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CompraDto
{
    public int Id_Compras { get; set; }
    public string? Proveedor { get; set; }
    public DateTime Fecha { get; set; }
    public string? Comprobante { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "";
    public string? MotivoAnulacion { get; set; }
    public List<LineaDto> Detalles { get; set; } = [];
}
