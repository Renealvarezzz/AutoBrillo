namespace AutoBrillo.Api.Contratos;

/// <summary>Indica si un usuario puede iniciar sesión y operar en la aplicación.</summary>
public record EstadoUsuarioRequest(bool Activo);
