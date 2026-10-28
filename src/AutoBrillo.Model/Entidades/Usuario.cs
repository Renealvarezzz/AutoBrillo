namespace AutoBrillo.Model.Entidades;

/// <summary>Representa un usuario del sistema.</summary>
public class Usuario
{
    /// <summary>Identificador único generado automáticamente por PostgreSQL.</summary>
    public int Id_Usuarios { get; set; }

    /// <summary>Nombre que la persona escribe para iniciar sesión.</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. Nunca guarda la contraseña original.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Indica si el usuario puede iniciar sesión y usar la aplicación.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Roles asignados al usuario mediante la tabla intermedia usuarios_roles.</summary>
    public List<Rol> Roles { get; set; } = [];
}
