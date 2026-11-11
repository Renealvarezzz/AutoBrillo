using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Vehiculos
{
    private List<PersonaDto> clientes = [];
    private List<TipoVehiculoDto> tipos = [];
    private List<VehiculoDto> vehiculos = [];
    private int idVehiculo;
    private int idCliente;
    private int idTipo;
    private string patente = string.Empty;
    private string? marca;
    private string? modelo;
    private string? color;
    private bool activo = true;

    protected override async Task CargarAsync()
    {
        clientes = (await Gestion.ObtenerPersonasAsync() ?? []).Where(p => p.Activo && p.Tipos.Contains("Cliente")).ToList();
        tipos = await Operaciones.ObtenerAsync<List<TipoVehiculoDto>>("api/tipos-vehiculos") ?? [];
        vehiculos = await Operaciones.ObtenerAsync<List<VehiculoDto>>("api/vehiculos") ?? [];
    }

    private async Task Guardar()
    {
        if (idCliente == 0 || idTipo == 0 || string.IsNullOrWhiteSpace(patente))
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Completá cliente, tipo y patente.");
            return;
        }

        var datos = new { id_Personas = idCliente, id_Tipos_Vehiculos = idTipo, patente, marca, modelo, color, activo };
        var error = idVehiculo == 0
            ? await Operaciones.PostAsync("api/vehiculos", datos)
            : await Operaciones.PutAsync($"api/vehiculos/{idVehiculo}", datos);
        if (await HuboErrorAsync("No se pudo guardar el vehículo", error)) return;
        Limpiar();
        await CargarAsync();
    }

    private void Editar(VehiculoDto vehiculo)
    {
        idVehiculo = vehiculo.Id_Vehiculos;
        idCliente = vehiculo.Id_Personas;
        idTipo = vehiculo.Id_Tipos_Vehiculos;
        patente = vehiculo.Patente;
        marca = vehiculo.Marca;
        modelo = vehiculo.Modelo;
        color = vehiculo.Color;
        activo = vehiculo.Activo;
    }

    private async Task CambiarEstado(VehiculoDto vehiculo)
    {
        if (await HuboErrorAsync("No se pudo cambiar el estado", await Operaciones.PatchAsync($"api/vehiculos/{vehiculo.Id_Vehiculos}/estado", new { activo = !vehiculo.Activo }))) return;
        await CargarAsync();
    }

    private void Limpiar()
    {
        idVehiculo = 0;
        idCliente = 0;
        idTipo = 0;
        patente = string.Empty;
        marca = modelo = color = null;
        activo = true;
    }
}
