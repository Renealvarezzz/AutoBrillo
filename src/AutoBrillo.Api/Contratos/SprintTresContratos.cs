namespace AutoBrillo.Api.Contratos;

public record LavadoRequest(int Id_Vehiculos, int Sector, DateTime Fecha, List<int> Id_Servicios, List<int> Id_Adicionales, List<int> Id_Participantes);
public record VentaLineaRequest(int Id_Productos, decimal Cantidad);
public record VentaRequest(int? Id_Lavados, string Condicion, List<VentaLineaRequest> Detalles);
public record CobroRequest(string MedioPago, decimal Importe);
public record ConfiguracionComisionRequest(decimal Porcentaje);
