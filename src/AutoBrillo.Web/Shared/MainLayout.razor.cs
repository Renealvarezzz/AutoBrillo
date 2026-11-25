using AutoBrillo.Web.Servicios;
using Microsoft.JSInterop;

namespace AutoBrillo.Web.Shared;

/// <summary>
/// Código del menú superior, separado de su vista MainLayout.razor.
/// Se encarga de mostrar el usuario conectado y de cerrar la sesión.
/// </summary>
public partial class MainLayout
{
    private string? usuario;
    private bool esAdministrador;
    private bool esStock;
    private bool esOperacion;
    private bool sesionLista;
    private bool adminIniciado;

    /// <summary>Obtiene el nombre y los roles de la sesión cuando se crea el diseño de la página.</summary>
    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync())
        {
            sesionLista = true;
            return;
        }

        usuario = await Autenticacion.ObtenerUsuarioAsync();
        var roles = await Autenticacion.ObtenerRolesAsync();
        esAdministrador = roles.Contains(RolesPantalla.Administrador, StringComparer.OrdinalIgnoreCase);
        esStock = roles.Contains(RolesPantalla.Stock, StringComparer.OrdinalIgnoreCase);
        esOperacion = esAdministrador
            || roles.Contains(RolesPantalla.Supervisor, StringComparer.OrdinalIgnoreCase)
            || roles.Contains(RolesPantalla.Caja, StringComparer.OrdinalIgnoreCase);
        sesionLista = true;
    }

    /// <summary>Activa el shell de AdminLTE cuando ya se conoce la sesión. El login no lo usa.</summary>
    protected override void OnAfterRender(bool firstRender)
    {
        if (!sesionLista || JS is not IJSInProcessRuntime js) return;
        var autenticado = !string.IsNullOrWhiteSpace(usuario);
        js.InvokeVoid("abAdmin.modo", autenticado);
        if (!autenticado || adminIniciado) return;
        adminIniciado = true;
        js.InvokeVoid("abAdmin.iniciar");
    }

    /// <summary>
    /// Borra token y nombre del navegador mediante ServicioAutenticacion,
    /// luego devuelve a la pantalla Login.
    /// </summary>
    private async Task Salir()
    {
        await Autenticacion.CerrarSesionAsync();
        Navegacion.NavigateTo("login", forceLoad: true);
    }
}
