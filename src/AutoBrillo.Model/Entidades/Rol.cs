namespace AutoBrillo.Model.Entidades;

/// <summary>Representa un rol del sistema. Un rol puede pertenecer a varios usuarios.</summary>
public class Rol
{
    /// <summary>Identificador único autoincremental de la tabla roles.</summary>
    public int Id_Roles { get; set; }

    /// <summary>Nombre descriptivo del rol, por ejemplo Administrador o Caja.</summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Usuarios que tienen asignado este rol.</summary>
    public List<Usuario> Usuarios { get; set; } = [];
}
