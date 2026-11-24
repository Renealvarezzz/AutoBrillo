using AutoBrillo.Web.Servicios;
namespace AutoBrillo.Web.Pages;
public partial class Comisiones
{
    private decimal porcentaje=30; private List<ComisionSprintDto> comisiones=[];
    protected override async Task CargarAsync(){var datos=await Operaciones.ObtenerAsync<ComisionesSprintDto>("api/comisiones");if(datos is not null){porcentaje=datos.Porcentaje;comisiones=datos.Items;}}
    private async Task Guardar(){if(await HuboErrorAsync("No se pudo guardar",await Operaciones.PutAsync("api/comisiones/porcentaje",new{porcentaje})))return;await CargarAsync();}
}
