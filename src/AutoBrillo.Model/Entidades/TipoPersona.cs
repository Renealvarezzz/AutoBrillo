namespace AutoBrillo.Model.Entidades;

/// <summary>Clasificación reutilizable de una persona, por ejemplo Cliente o Proveedor.</summary>
public class TipoPersona
{
    public int Id_Tipos_Personas { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public List<Persona> Personas { get; set; } = [];
}
