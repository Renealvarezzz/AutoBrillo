namespace AutoBrillo.Api.Contratos;

public record ProductoResponse(int Id_Productos, string Descripcion, decimal PrecioDeCosto, decimal PrecioDeVenta, bool Activo, decimal StockActual, List<int> Id_Tipos_Productos, List<string> Tipos);
