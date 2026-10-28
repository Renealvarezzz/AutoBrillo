namespace AutoBrillo.Web.Servicios;

/// <summary>Valida la repetición de contraseña antes de enviar el usuario a la API.</summary>
public static class ValidadorPasswordUsuario
{
    public static bool Coinciden(string password, string confirmacion)
        => string.Equals(password, confirmacion, StringComparison.Ordinal);
}
