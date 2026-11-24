using AutoBrillo.Web.Servicios;
namespace AutoBrillo.Web.Pages;
public partial class Lavados
{
    private List<VehiculoDto> vehiculos=[]; private List<ProductoDto> productos=[]; private List<PersonaDto> funcionarios=[]; private List<LavadoSprintDto> lavados=[];
    private int vehiculo,sector=1,servicio,adicional,idEdicion; private DateTime fecha=DateTime.Now; private string[] funcionariosSeleccionados=[];
    protected override async Task CargarAsync(){vehiculos=(await Operaciones.ObtenerAsync<List<VehiculoDto>>("api/vehiculos")??[]).Where(x=>x.Activo).ToList();productos=(await Gestion.ObtenerProductosAsync()??[]).ToList();funcionarios=(await Gestion.ObtenerPersonasAsync()??[]).Where(x=>x.Activo&&x.Tipos.Contains("Funcionario")).ToList();lavados=await Operaciones.ObtenerAsync<List<LavadoSprintDto>>("api/lavados")??[];}
    private async Task Registrar(){if(vehiculo==0||servicio==0||funcionariosSeleccionados.Length==0){await Alertas.MostrarErrorGeneralAsync("Revisá los datos","Seleccioná vehículo, servicio y uno o más funcionarios.");return;}var datos=new{id_Vehiculos=vehiculo,sector,fecha,id_Servicios=new[]{servicio},id_Adicionales=adicional==0?Array.Empty<int>():new[]{adicional},id_Participantes=funcionariosSeleccionados.Select(int.Parse)};var e=idEdicion==0?await Operaciones.PostAsync("api/lavados",datos):await Operaciones.PutAsync($"api/lavados/{idEdicion}",datos);if(await HuboErrorAsync("No se pudo guardar el lavado",e))return;Limpiar();await CargarAsync();}
    private void Editar(LavadoSprintDto l){idEdicion=l.Id_Lavados;vehiculo=l.Id_Vehiculos;sector=l.Sector;fecha=l.Fecha;servicio=l.Servicios.FirstOrDefault(x=>!x.EsAdicional)?.Id_Productos??0;adicional=l.Servicios.FirstOrDefault(x=>x.EsAdicional)?.Id_Productos??0;funcionariosSeleccionados=l.Participantes.Select(x=>x.Id_Personas.ToString()).ToArray();}
    private void Limpiar(){idEdicion=0;vehiculo=0;sector=1;servicio=adicional=0;fecha=DateTime.Now;funcionariosSeleccionados=[];}
    private async Task Finalizar(LavadoSprintDto l){if(await HuboErrorAsync("No se pudo terminar",await Operaciones.PostAsync($"api/lavados/{l.Id_Lavados}/finalizar",new{})))return;await CargarAsync();}
    private async Task Cancelar(LavadoSprintDto l){var motivo=await JSInteropPrompt();if(string.IsNullOrWhiteSpace(motivo))return;if(await HuboErrorAsync("No se pudo cancelar",await Operaciones.PostAsync($"api/lavados/{l.Id_Lavados}/cancelar",new{motivo})))return;await CargarAsync();}
    private async Task<string?> JSInteropPrompt()=>await JS.InvokeAsync<string?>("prompt",new object?[]{"Motivo de cancelación del lavado:"});
}
