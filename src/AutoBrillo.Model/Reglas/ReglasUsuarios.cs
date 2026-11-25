using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Model.Reglas;

/// <summary>Protege que el lavadero no se quede sin un administrador que pueda entrar.</summary>
public static class ReglasUsuarios
{
    public const string Administrador = "Administrador";

    public static bool EsAdministradorActivo(Usuario usuario) =>
        usuario.Activo && TieneRolAdministrador(usuario);

    public static bool EsRolAdministrador(Rol rol) =>
        string.Equals(rol.Descripcion, Administrador, StringComparison.OrdinalIgnoreCase);

    public static bool TieneRolAdministrador(Usuario usuario) =>
        usuario.Roles.Any(EsRolAdministrador);

    /// <summary>
    /// Verdadero cuando el usuario afectado es hoy un administrador activo
    /// y, después del cambio, no quedaría ningún otro administrador activo.
    /// </summary>
    public static bool QuedariaSinAdministradorActivo(IEnumerable<Usuario> usuarios, int idAfectado, bool seguiraActivo, bool seguiraSiendoAdministrador)
    {
        var lista = usuarios.ToList();
        var afectado = lista.FirstOrDefault(usuario => usuario.Id_Usuarios == idAfectado);
        if (afectado is null || !EsAdministradorActivo(afectado)) return false;
        if (seguiraActivo && seguiraSiendoAdministrador) return false;
        return !lista.Any(usuario => usuario.Id_Usuarios != idAfectado && EsAdministradorActivo(usuario));
    }
}
