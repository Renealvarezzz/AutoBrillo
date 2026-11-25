using AutoBrillo.Web.Servicios;
using Microsoft.AspNetCore.Components;
namespace AutoBrillo.Web.Pages;
public partial class Ventas
{
    private List<ProductoDto> productos=[]; private List<LavadoSprintDto> lavados=[]; private List<VentaSprintDto> ventas=[]; private List<VentaLineaEdicion> lineas=[];
    private readonly Dictionary<int, CobroEdicion> cobros = [];
    private int idLavado,producto; private decimal cantidad=1; private string condicion="Contado";
    protected override async Task CargarAsync(){productos=(await Gestion.ObtenerProductosAsync()??[]).ToList();lavados=await Operaciones.ObtenerAsync<List<LavadoSprintDto>>("api/lavados")??[];ventas=await Operaciones.ObtenerAsync<List<VentaSprintDto>>("api/ventas")??[];}
    private void Agregar(){if(producto==0||cantidad<=0)return;lineas.Add(new(producto,cantidad));producto=0;cantidad=1;}
    private void CambiarLavado(ChangeEventArgs e)
    {
        idLavado = int.TryParse(e.Value?.ToString(), out var id) ? id : 0;
        lineas = [];
        if (idLavado == 0) return;
        var lavado = lavados.FirstOrDefault(x => x.Id_Lavados == idLavado);
        if (lavado is null) return;
        foreach (var servicio in lavado.Servicios.OrderBy(x => x.EsAdicional))
            lineas.Add(new(servicio.Id_Productos, 1));
    }
    private bool LavadoDisponibleParaVenta(LavadoSprintDto lavado) =>
        lavado.Estado is "Registrado" or "Terminado" && !ventas.Any(v => v.Id_Lavados == lavado.Id_Lavados && v.Estado != "Anulada");
    private async Task Registrar(){if(lineas.Count==0){await Alertas.MostrarErrorGeneralAsync("Revisá los datos","Agregá al menos un producto o servicio.");return;}var err=await Operaciones.PostAsync("api/ventas",new{id_Lavados=idLavado==0?(int?)null:idLavado,condicion,detalles=lineas.Select(x=>new{id_Productos=x.Id_Productos,cantidad=x.Cantidad})});if(await HuboErrorAsync("No se pudo registrar la venta",err))return;lineas=[];idLavado=0;await CargarAsync();}
    private async Task Cobrar(VentaSprintDto v){var cobro=CobroDe(v.Id_Ventas);if(cobro.Importe<=0){await Alertas.MostrarErrorGeneralAsync("Revisá el importe","Ingresá un importe de cobro válido.");return;}if(await HuboErrorAsync("No se pudo registrar el cobro",await Operaciones.PostAsync($"api/ventas/{v.Id_Ventas}/cobros",new{medioPago=cobro.Medio,importe=cobro.Importe})))return;cobro.Importe=0;await CargarAsync();}
    private async Task Anular(VentaSprintDto v){var motivo=await PedirMotivoAsync($"la venta #{v.Id_Ventas}");if(string.IsNullOrWhiteSpace(motivo))return;if(await HuboErrorAsync("No se pudo anular",await Operaciones.PostAsync($"api/ventas/{v.Id_Ventas}/anular",new{motivo})))return;await CargarAsync();}
    private CobroEdicion CobroDe(int id){if(!cobros.TryGetValue(id,out var cobro)) cobros[id]=cobro=new(); return cobro;}
    private string Medio(int id)=>CobroDe(id).Medio;
    private decimal Importe(int id)=>CobroDe(id).Importe;
    private void FijarMedio(int id, ChangeEventArgs e)=>CobroDe(id).Medio=e.Value?.ToString()??"Efectivo";
    private void FijarImporte(int id, ChangeEventArgs e)=>CobroDe(id).Importe=decimal.TryParse(Convert.ToString(e.Value), out var importe)?importe:0;
    private record VentaLineaEdicion(int Id_Productos,decimal Cantidad);
    private sealed class CobroEdicion { public string Medio { get; set; } = "Efectivo"; public decimal Importe { get; set; } }
}
