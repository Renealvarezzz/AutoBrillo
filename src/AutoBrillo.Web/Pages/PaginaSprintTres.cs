using AutoBrillo.Web.Servicios;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AutoBrillo.Web.Pages;

public abstract class PaginaSprintTres : ComponentBase
{
    [Inject] protected ServicioAutenticacion Autenticacion { get; set; } = default!;
    [Inject] protected ServicioOperaciones Operaciones { get; set; } = default!;
    [Inject] protected ServicioGestion Gestion { get; set; } = default!;
    [Inject] protected ServicioAlertas Alertas { get; set; } = default!;
    [Inject] protected NavigationManager Navegacion { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    protected bool cargando = true;
    protected bool esAdministrador;
    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        esAdministrador = await Autenticacion.EsAdministradorAsync();
        await CargarAsync(); cargando = false;
    }
    protected abstract Task CargarAsync();
    protected async Task<bool> HuboErrorAsync(string titulo, string? error)
    {
        if (error is null) return false;
        await Alertas.MostrarErrorGeneralAsync(titulo, error); return true;
    }
    protected static string Gs(decimal n) => n.ToString("N0");
    protected static string Fecha(DateTime f) => f.ToString("dd/MM/yyyy HH:mm");
}
