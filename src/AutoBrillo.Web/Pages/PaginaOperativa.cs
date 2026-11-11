using AutoBrillo.Web.Servicios;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AutoBrillo.Web.Pages;

/// <summary>Base de las pantallas del sprint 2: valida la sesión de administrador y ofrece ayudas comunes.</summary>
public abstract class PaginaOperativa : ComponentBase
{
    [Inject] protected ServicioAutenticacion Autenticacion { get; set; } = default!;
    [Inject] protected ServicioOperaciones Operaciones { get; set; } = default!;
    [Inject] protected ServicioGestion Gestion { get; set; } = default!;
    [Inject] protected ServicioAlertas Alertas { get; set; } = default!;
    [Inject] protected NavigationManager Navegacion { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    protected bool cargando = true;

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        if (!await Autenticacion.EsAdministradorAsync()) { Navegacion.NavigateTo("inicio", true); return; }
        await CargarAsync();
        cargando = false;
    }

    protected abstract Task CargarAsync();

    protected async Task<bool> HuboErrorAsync(string titulo, string? error)
    {
        if (error is null) return false;
        await Alertas.MostrarErrorGeneralAsync(titulo, error);
        return true;
    }

    protected async Task<string?> PedirMotivoAsync(string que)
    {
        var motivo = await JS.InvokeAsync<string?>("prompt", $"Motivo de la anulación de {que}:");
        return string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
    }

    protected static string Gs(decimal valor) => valor.ToString("N0");
    protected static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm");
}
