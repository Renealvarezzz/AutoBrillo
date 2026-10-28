namespace AutoBrillo.Api.Contratos;

public record PersonaRequest(string Nombre, string Documento, string? Telefono, List<int> Id_Tipos_Personas);
