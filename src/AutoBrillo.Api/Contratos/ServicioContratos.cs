namespace AutoBrillo.Api.Contratos;

public record CatalogoRequest(string Descripcion, bool Activo = true);
public record PrecioServicioRequest(int Id_Productos, int Id_Tipos_Vehiculos, decimal Precio);
public record ServicioInsumoRequest(int Id_Servicios, int Id_Insumos, decimal Cantidad);
