using AutoBrillo.Web.Servicios;
using Microsoft.AspNetCore.Components;

namespace AutoBrillo.Web.Pages;

/// <summary>Base de las pantallas operativas: valida la sesión, el rol y muestra un 403 en lugar de una lista vacía.</summary>
public abstract class PaginaOperativa : ComponentBase
{
    [Inject] protected ServicioAutenticacion Autenticacion { get; set; } = default!;
    [Inject] protected ServicioOperaciones Operaciones { get; set; } = default!;
    [Inject] protected ServicioGestion Gestion { get; set; } = default!;
    [Inject] protected ServicioAlertas Alertas { get; set; } = default!;
    [Inject] protected NavigationManager Navegacion { get; set; } = default!;

    protected bool cargando = true;
    protected bool esAdministrador;
    protected string? mensajeAcceso;

    /// <summary>Roles que pueden abrir la pantalla. Las hijas lo reemplazan cuando no es solo el administrador.</summary>
    protected virtual string[] RolesPermitidos => [RolesPantalla.Administrador];

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        esAdministrador = await Autenticacion.EsAdministradorAsync();
        if (!await Autenticacion.TieneAlgunRolAsync(RolesPermitidos))
        {
            await Alertas.MostrarErrorGeneralAsync("Acceso denegado", ServicioOperaciones.MensajeSinPermiso);
            Navegacion.NavigateTo("inicio");
            return;
        }

        try { await CargarAsync(); }
        catch (AccesoDenegadoException ex)
        {
            mensajeAcceso = ex.Message;
            await Alertas.MostrarErrorGeneralAsync("Acceso denegado", ex.Message);
        }

        cargando = false;
    }

    protected abstract Task CargarAsync();

    protected async Task<bool> HuboErrorAsync(string titulo, string? error)
    {
        if (error is null) return false;
        await Alertas.MostrarErrorGeneralAsync(titulo, error);
        return true;
    }

    protected Task<string?> PedirMotivoAsync(string que) =>
        Alertas.PedirTextoAsync("Motivo de anulación", $"Indicá por qué se anula {que}.");

    protected static string Gs(decimal valor) => valor.ToString("N0");
    protected static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm");
}
