using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Compras
{
    private List<ProductoDto> productos = [];
    private List<PersonaDto> proveedores = [];
    private List<CompraDto> compras = [];
    private List<LineaDto> lineas = [];
    private int idProveedor;
    private string? comprobante;

    protected override async Task CargarAsync()
    {
        productos = (await Gestion.ObtenerProductosAsync() ?? [])
            .Where(p => p.Activo && (p.Tipos.Count == 0 || p.Tipos.Any(t => t is "Producto" or "Insumo")))
            .ToList();
        proveedores = (await Gestion.ObtenerPersonasAsync() ?? []).Where(p => p.Activo && p.Tipos.Contains("Proveedor")).ToList();
        compras = await Operaciones.ObtenerAsync<List<CompraDto>>("api/compras") ?? [];
    }

    private async Task Registrar()
    {
        if (idProveedor == 0 || lineas.Count == 0)
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná un proveedor y agregá al menos un producto.");
            return;
        }

        var error = await Operaciones.PostAsync("api/compras", new { id_Personas = idProveedor, comprobante, detalles = lineas.Select(l => new { l.Id_Productos, l.Cantidad, l.PrecioUnitario }) });
        if (await HuboErrorAsync("No se pudo registrar la compra", error)) return;
        lineas = [];
        comprobante = null;
        idProveedor = 0;
        await CargarAsync();
    }

    private async Task Anular(CompraDto compra)
    {
        var motivo = await PedirMotivoAsync($"la compra #{compra.Id_Compras}");
        if (motivo is null) return;
        if (await HuboErrorAsync("No se pudo anular", await Operaciones.PostAsync($"api/compras/{compra.Id_Compras}/anular", new { motivo }))) return;
        await CargarAsync();
    }
}
