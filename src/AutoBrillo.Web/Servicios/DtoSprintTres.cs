namespace AutoBrillo.Web.Servicios;
public class LavadoSprintDto
{
    public int Id_Lavados { get; set; } public int Id_Vehiculos { get; set; } public string Vehiculo { get; set; } = ""; public string Cliente { get; set; } = "";
    public int Sector { get; set; } public DateTime Fecha { get; set; } public string Estado { get; set; } = "";
    public List<ParticipanteSprintDto> Participantes { get; set; } = []; public List<ServicioLavadoSprintDto> Servicios { get; set; } = [];
}
public class ParticipanteSprintDto { public int Id_Personas { get; set; } public string Nombre { get; set; } = ""; }
public class ServicioLavadoSprintDto { public int Id_Productos { get; set; } public string Producto { get; set; } = ""; public decimal PrecioAplicado { get; set; } public bool EsAdicional { get; set; } }
public class VentaSprintDto
{
    public int Id_Ventas { get; set; } public int? Id_Lavados { get; set; } public DateTime Fecha { get; set; } public string Condicion { get; set; } = ""; public string Usuario { get; set; } = "";
    public decimal Total { get; set; } public decimal Cobrado { get; set; } public string Estado { get; set; } = "";
    public List<LineaDto> Detalles { get; set; } = [];
}
public class ComisionesSprintDto { public decimal Porcentaje { get; set; } public List<ComisionSprintDto> Items { get; set; } = []; }
public class ComisionSprintDto { public int Id_Ventas { get; set; } public string Funcionario { get; set; } = ""; public decimal PorcentajeAplicado { get; set; } public decimal Importe { get; set; } public bool Anulada { get; set; } }
