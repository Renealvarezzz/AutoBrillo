using AutoBrillo.Web.Servicios;
namespace AutoBrillo.Web.Pages;
public partial class Ventas
{
    private List<ProductoDto> productos=[]; private List<LavadoSprintDto> lavados=[]; private List<VentaSprintDto> ventas=[]; private List<VentaLineaEdicion> lineas=[];
    private int idLavado,producto; private decimal cantidad=1,importeCobro; private string condicion="Contado",medio="Efectivo";
    protected override async Task CargarAsync(){productos=(await Gestion.ObtenerProductosAsync()??[]).ToList();lavados=await Operaciones.ObtenerAsync<List<LavadoSprintDto>>("api/lavados")??[];ventas=await Operaciones.ObtenerAsync<List<VentaSprintDto>>("api/ventas")??[];}
    private void Agregar(){if(producto==0||cantidad<=0)return;lineas.Add(new(producto,cantidad));producto=0;cantidad=1;}
    private async Task Registrar(){if(lineas.Count==0){await Alertas.MostrarErrorGeneralAsync("Revisá los datos","Agregá al menos un producto o servicio.");return;}var err=await Operaciones.PostAsync("api/ventas",new{id_Lavados=idLavado==0?(int?)null:idLavado,condicion,detalles=lineas.Select(x=>new{id_Productos=x.Id_Productos,cantidad=x.Cantidad})});if(await HuboErrorAsync("No se pudo registrar la venta",err))return;lineas=[];idLavado=0;await CargarAsync();}
    private async Task Cobrar(VentaSprintDto v){if(importeCobro<=0){await Alertas.MostrarErrorGeneralAsync("Revisá el importe","Ingresá un importe de cobro válido.");return;}if(await HuboErrorAsync("No se pudo registrar el cobro",await Operaciones.PostAsync($"api/ventas/{v.Id_Ventas}/cobros",new{medioPago=medio,importe=importeCobro})))return;importeCobro=0;await CargarAsync();}
    private async Task Anular(VentaSprintDto v){var motivo=await JS.InvokeAsync<string?>("prompt",new object?[]{"Motivo de anulación de la venta:"});if(string.IsNullOrWhiteSpace(motivo))return;if(await HuboErrorAsync("No se pudo anular",await Operaciones.PostAsync($"api/ventas/{v.Id_Ventas}/anular",new{motivo})))return;await CargarAsync();}
    private record VentaLineaEdicion(int Id_Productos,decimal Cantidad);
}
