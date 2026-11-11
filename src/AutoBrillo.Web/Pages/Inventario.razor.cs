using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Inventario
{
    private List<ProductoDto> productos = [];
    private List<MovimientoStockDto> movimientos = [];
    private int filtroProducto;
    private Nuevo nuevo = new();

    private sealed class Nuevo
    {
        public int Id_Productos { get; set; }
        public string Origen { get; set; } = "Consumo interno";
        public decimal Cantidad { get; set; } = 1;
        public string? Observacion { get; set; }
    }

    protected override async Task CargarAsync()
    {
        productos = await Gestion.ObtenerProductosAsync() ?? [];
        movimientos = await Operaciones.ObtenerAsync<List<MovimientoStockDto>>("api/movimientos-stock" + (filtroProducto > 0 ? $"?idProducto={filtroProducto}" : "")) ?? [];
    }

    private async Task Registrar()
    {
        if (nuevo.Id_Productos == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná un producto."); return; }
        var tipo = nuevo.Origen == "Ajuste positivo" ? "Entrada" : "Salida";
        var error = await Operaciones.PostAsync("api/movimientos-stock", new { id_Productos = nuevo.Id_Productos, tipo, origen = nuevo.Origen, cantidad = nuevo.Cantidad, observacion = nuevo.Observacion });
        if (await HuboErrorAsync("No se pudo registrar el movimiento", error)) return;
        nuevo = new();
        await CargarAsync();
    }
}
