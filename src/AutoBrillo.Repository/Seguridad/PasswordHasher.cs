namespace AutoBrillo.Repository.Seguridad;

/// <summary>
/// Genera y verifica hashes BCrypt para contraseñas.
/// BCrypt transforma una contraseña en un valor no reversible antes de guardarla.
/// </summary>
public class PasswordHasher
{
    /// <summary>
    /// Convierte una contraseña escrita por el usuario en un hash BCrypt.
    /// Ejemplo: "clave123" se guarda como una cadena BCrypt, no como "clave123".
    /// </summary>
    public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// Comprueba si la contraseña escrita coincide con el hash guardado en PostgreSQL.
    /// Devuelve true cuando coincide y false cuando es incorrecta.
    /// </summary>
    public bool Verificar(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
