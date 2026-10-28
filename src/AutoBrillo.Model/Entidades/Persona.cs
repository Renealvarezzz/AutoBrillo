namespace AutoBrillo.Model.Entidades;

/// <summary>Persona que puede ser cliente, funcionario, proveedor o varios tipos a la vez.</summary>
public class Persona
{
    public int Id_Personas { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public List<TipoPersona> Tipos { get; set; } = [];
}
