namespace AutoBrillo.Api.Contratos;

public record CompraDetalleRequest(int Id_Productos, decimal Cantidad, decimal PrecioUnitario);
public record CompraRequest(int Id_Personas, string? Comprobante, List<CompraDetalleRequest>? Detalles);
public record AnulacionRequest(string? Motivo);
public record MovimientoStockRequest(int Id_Productos, string Tipo, string? Origen, decimal Cantidad, string? Observacion);
