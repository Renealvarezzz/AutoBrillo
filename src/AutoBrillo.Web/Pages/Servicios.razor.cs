using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Servicios
{
    private List<ProductoDto> productos = [];
    private List<TipoProductoDto> tiposProducto = [];
    private List<TipoVehiculoDto> tipos = [];
    private List<PrecioServicioDto> precios = [];
    private List<ServicioInsumoDto> configuracion = [];
    private TipoVehiculoDto tipoEdicion = new();
    private int idProductoPrecio;
    private int idTipoPrecio;
    private decimal precio;
    private int idAdicional;
    private string adicionalNombre = string.Empty;
    private decimal adicionalPrecio;
    private int idServicioInsumo;
    private int idInsumo;
    private decimal cantidadInsumo = 1;
    private int idServicioPrecio;
    private int idTipoCalculo;
    private HashSet<int> adicionalesElegidos = [];
    private PrecioLavadoDto? precioCalculado;

    private IEnumerable<ProductoDto> servicios => productos.Where(p => p.Tipos.Contains("Servicio"));
    private IEnumerable<ProductoDto> adicionales => productos.Where(p => p.Tipos.Contains("Adicional"));
    private IEnumerable<ProductoDto> cotizables => productos.Where(p => p.Tipos.Contains("Servicio") || p.Tipos.Contains("Adicional"));
    private IEnumerable<ProductoDto> insumos => productos.Where(p => p.Activo && p.Tipos.Contains("Insumo"));

    protected override async Task CargarAsync()
    {
        productos = await Gestion.ObtenerProductosAsync() ?? [];
        tiposProducto = await Gestion.ObtenerTiposProductosAsync() ?? [];
        tipos = await Operaciones.ObtenerAsync<List<TipoVehiculoDto>>("api/tipos-vehiculos") ?? [];
        precios = await Operaciones.ObtenerAsync<List<PrecioServicioDto>>("api/precios-servicios") ?? [];
        configuracion = await Operaciones.ObtenerAsync<List<ServicioInsumoDto>>("api/servicios/insumos") ?? [];
    }

    private async Task GuardarPrecio()
    {
        if (idProductoPrecio == 0 || idTipoPrecio == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná servicio y tipo de vehículo."); return; }
        if (await HuboErrorAsync("No se pudo guardar el precio", await Operaciones.PutAsync("api/precios-servicios", new { id_Productos = idProductoPrecio, id_Tipos_Vehiculos = idTipoPrecio, precio }))) return;
        idProductoPrecio = 0; idTipoPrecio = 0; precio = 0;
        await CargarAsync();
    }

    private void EditarPrecio(PrecioServicioDto item)
    {
        idProductoPrecio = item.Id_Productos;
        idTipoPrecio = item.Id_Tipos_Vehiculos;
        precio = item.Precio;
    }

    private async Task EliminarPrecio(PrecioServicioDto item)
    {
        if (await HuboErrorAsync("No se pudo eliminar", await Operaciones.DeleteAsync($"api/precios-servicios/{item.Id_Precios_Servicios}"))) return;
        await CargarAsync();
    }

    private async Task GuardarTipo()
    {
        if (string.IsNullOrWhiteSpace(tipoEdicion.Descripcion)) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Escribí el nombre del tipo."); return; }
        var datos = new { descripcion = tipoEdicion.Descripcion.Trim(), activo = tipoEdicion.Id_Tipos_Vehiculos == 0 || tipoEdicion.Activo };
        var error = tipoEdicion.Id_Tipos_Vehiculos == 0
            ? await Operaciones.PostAsync("api/tipos-vehiculos", datos)
            : await Operaciones.PutAsync($"api/tipos-vehiculos/{tipoEdicion.Id_Tipos_Vehiculos}", datos);
        if (await HuboErrorAsync("No se pudo guardar el tipo", error)) return;
        tipoEdicion = new();
        await CargarAsync();
    }

    private async Task CambiarEstadoTipo(TipoVehiculoDto tipo)
    {
        if (await HuboErrorAsync("No se pudo cambiar el estado", await Operaciones.PutAsync($"api/tipos-vehiculos/{tipo.Id_Tipos_Vehiculos}", new { descripcion = tipo.Descripcion, activo = !tipo.Activo }))) return;
        await CargarAsync();
    }

    private async Task GuardarAdicional()
    {
        var tipo = tiposProducto.FirstOrDefault(t => t.Descripcion == "Adicional");
        if (tipo is null) { await Alertas.MostrarErrorGeneralAsync("Falta el tipo Adicional", "Aplicá las migraciones de la base antes de cargar adicionales."); return; }
        if (string.IsNullOrWhiteSpace(adicionalNombre)) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Escribí el nombre del adicional."); return; }
        var actual = productos.FirstOrDefault(p => p.Id_Productos == idAdicional);
        var error = await Gestion.GuardarProductoAsync(new ProductoEdicionDto
        {
            Id_Productos = idAdicional,
            Descripcion = adicionalNombre.Trim(),
            PrecioDeCosto = actual?.PrecioDeCosto ?? 0,
            PrecioDeVenta = adicionalPrecio,
            StockActual = actual?.StockActual ?? 0,
            Id_Tipos_Productos = actual?.Id_Tipos_Productos.Contains(tipo.Id_Tipos_Productos) == true ? actual.Id_Tipos_Productos : [tipo.Id_Tipos_Productos]
        });
        if (await HuboErrorAsync("No se pudo guardar el adicional", error)) return;
        LimpiarAdicional();
        await CargarAsync();
    }

    private void EditarAdicional(ProductoDto producto)
    {
        idAdicional = producto.Id_Productos;
        adicionalNombre = producto.Descripcion;
        adicionalPrecio = producto.PrecioDeVenta;
    }

    private void LimpiarAdicional()
    {
        idAdicional = 0;
        adicionalNombre = string.Empty;
        adicionalPrecio = 0;
    }

    private async Task GuardarInsumo()
    {
        if (idServicioInsumo == 0 || idInsumo == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná el servicio y el insumo."); return; }
        if (await HuboErrorAsync("No se pudo guardar", await Operaciones.PutAsync("api/servicios/insumos", new { id_Servicios = idServicioInsumo, id_Insumos = idInsumo, cantidad = cantidadInsumo }))) return;
        idServicioInsumo = 0; idInsumo = 0; cantidadInsumo = 1;
        await CargarAsync();
    }

    private async Task QuitarInsumo(ServicioInsumoDto fila)
    {
        if (await HuboErrorAsync("No se pudo quitar", await Operaciones.DeleteAsync($"api/servicios/insumos/{fila.Id_Servicios_Insumos}"))) return;
        await CargarAsync();
    }

    private void AlternarAdicional(int id)
    {
        if (!adicionalesElegidos.Add(id)) adicionalesElegidos.Remove(id);
        precioCalculado = null;
    }

    private async Task CalcularPrecio()
    {
        if (idServicioPrecio == 0 || idTipoCalculo == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná servicio y tipo de vehículo."); return; }
        var consulta = "api/servicios/precio?idServicio=" + idServicioPrecio + "&idTipoVehiculo=" + idTipoCalculo + string.Concat(adicionalesElegidos.Select(id => "&adicionales=" + id));
        precioCalculado = await Operaciones.ObtenerAsync<PrecioLavadoDto>(consulta);
        if (precioCalculado is null) await Alertas.MostrarErrorGeneralAsync("No se pudo calcular", "Revisá el servicio, el tipo de vehículo y los adicionales.");
    }

    private async Task ConsumirInsumos()
    {
        if (idServicioPrecio == 0) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Seleccioná el servicio cuyo consumo querés descontar."); return; }
        if (await HuboErrorAsync("No se pudo descontar el stock", await Operaciones.PostAsync($"api/servicios/{idServicioPrecio}/consumo", new { }))) return;
        await Alertas.MostrarAvisoAsync("Stock actualizado", "Se descontaron los insumos configurados para el servicio.");
        await CargarAsync();
    }
}
