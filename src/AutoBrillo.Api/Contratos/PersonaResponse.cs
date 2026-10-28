namespace AutoBrillo.Api.Contratos;

public record PersonaResponse(int Id_Personas, string Nombre, string Documento, string? Telefono, bool Activo, List<int> Id_Tipos_Personas, List<string> Tipos);
