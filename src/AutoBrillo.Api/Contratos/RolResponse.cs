namespace AutoBrillo.Api.Contratos;

/// <summary>Datos seguros de un rol para devolver a la web, sin sus usuarios asociados.</summary>
public record RolResponse(int Id_Roles, string Descripcion);
