namespace AutoBrillo.Api.Contratos;

public record ProductoRequest(string Descripcion, decimal PrecioDeCosto, decimal PrecioDeVenta, decimal StockActual, List<int> Id_Tipos_Productos);
