namespace AutoBrillo.Api.Contratos;

/// <summary>Datos seguros de un usuario para devolver a la web. Nunca incluye Password.</summary>
public record UsuarioResponse(int Id_Usuarios, string Nombre, List<int> Id_Roles, List<string> Roles, bool Activo);
