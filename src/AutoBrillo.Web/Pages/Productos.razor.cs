using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Productos
{
    private bool cargando = true;
    private List<ProductoDto> productos = [];
    private List<TipoProductoDto> tipos = [];
    private ProductoEdicionDto edicion = new();
    private TipoProductoDto tipoEdicion = new();

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        if (!await Autenticacion.EsAdministradorAsync()) { Navegacion.NavigateTo("inicio", true); return; }
        await CargarAsync(); cargando = false;
    }

    private async Task CargarAsync() { productos = await Gestion.ObtenerProductosAsync() ?? []; tipos = await Gestion.ObtenerTiposProductosAsync() ?? []; }
    private bool TipoSeleccionado(int id) => edicion.Id_Tipos_Productos.Contains(id);
    private string TextoTiposSeleccionados => edicion.Id_Tipos_Productos.Count == 0
        ? "Seleccionar tipos"
        : string.Join(", ", tipos.Where(tipo => TipoSeleccionado(tipo.Id_Tipos_Productos)).Select(tipo => tipo.Descripcion));

    private void CambiarTipo(int id)
    {
        if (TipoSeleccionado(id)) edicion.Id_Tipos_Productos.Remove(id);
        else edicion.Id_Tipos_Productos.Add(id);
    }

    private async Task GuardarProducto()
    {
        if (string.IsNullOrWhiteSpace(edicion.Descripcion) || edicion.Id_Tipos_Productos.Count == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Completá descripción y al menos un tipo."); return; }
        var error = await Gestion.GuardarProductoAsync(edicion);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo guardar el producto", error); return; }
        edicion = new(); await CargarAsync();
    }

    private void EditarProducto(ProductoDto producto) => edicion = new ProductoEdicionDto { Id_Productos = producto.Id_Productos, Descripcion = producto.Descripcion, PrecioDeCosto = producto.PrecioDeCosto, PrecioDeVenta = producto.PrecioDeVenta, StockActual = producto.StockActual, Id_Tipos_Productos = [.. producto.Id_Tipos_Productos] };
    private async Task CambiarEstado(ProductoDto producto)
    {
        var nuevoEstado = !producto.Activo;
        var accion = nuevoEstado ? "activar" : "desactivar";
        if (!await Alertas.ConfirmarCambioEstadoAsync("producto", accion, producto.Descripcion)) return;
        var error = await Gestion.CambiarEstadoProductoAsync(producto.Id_Productos, nuevoEstado);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo cambiar el estado", error); return; }
        await CargarAsync();
    }
    private async Task EliminarProducto(ProductoDto producto) { if (!await Alertas.ConfirmarEliminarUsuarioAsync(producto.Descripcion)) return; var error = await Gestion.EliminarProductoAsync(producto.Id_Productos); if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo eliminar el producto", error); return; } await CargarAsync(); }

    private async Task GuardarTipo()
    {
        if (string.IsNullOrWhiteSpace(tipoEdicion.Descripcion)) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Escribí la descripción del tipo."); return; }
        var error = await Gestion.GuardarTipoProductoAsync(tipoEdicion);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo guardar el tipo", error); return; }
        tipoEdicion = new(); await CargarAsync();
    }
    private void EditarTipo(TipoProductoDto tipo) => tipoEdicion = new TipoProductoDto { Id_Tipos_Productos = tipo.Id_Tipos_Productos, Descripcion = tipo.Descripcion };
    private async Task EliminarTipo(TipoProductoDto tipo) { if (!await Alertas.ConfirmarEliminarRolAsync(tipo.Descripcion)) return; var error = await Gestion.EliminarTipoProductoAsync(tipo.Id_Tipos_Productos); if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo eliminar el tipo", error); return; } if (tipoEdicion.Id_Tipos_Productos == tipo.Id_Tipos_Productos) tipoEdicion = new(); await CargarAsync(); }
}
