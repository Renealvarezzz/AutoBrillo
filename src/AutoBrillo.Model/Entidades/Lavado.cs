namespace AutoBrillo.Model.Entidades;

/// <summary>Lavado registrado para un vehículo, sector y hora.</summary>
public class Lavado : IOperacionAnulable
{
    public int Id_Lavados { get; set; }
    public int Id_Vehiculos { get; set; }
    public Vehiculo? Vehiculo { get; set; }
    public int Sector { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public string Estado { get; set; } = "Registrado";
    public int Id_Usuarios { get; set; }
    public DateTime? FechaAnulacion { get; set; }
    public int? Id_Usuarios_Anulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public List<LavadoServicio> Servicios { get; set; } = [];
    public List<LavadoParticipante> Participantes { get; set; } = [];
    public List<LavadoHistorialEstado> HistorialEstados { get; set; } = [];
}

public class LavadoHistorialEstado
{
    public int Id_Lavados_Historial { get; set; }
    public int Id_Lavados { get; set; }
    public string Estado { get; set; } = "";
    public DateTime Fecha { get; set; }
    public int Id_Usuarios { get; set; }
}

public class LavadoServicio
{
    public int Id_Lavados_Servicios { get; set; }
    public int Id_Lavados { get; set; }
    public int Id_Productos { get; set; }
    public Producto? Producto { get; set; }
    public decimal PrecioAplicado { get; set; }
    public bool EsAdicional { get; set; }
}

public class LavadoParticipante
{
    public int Id_Lavados { get; set; }
    public int Id_Personas { get; set; }
    public Persona? Persona { get; set; }
}
