using AutoBrillo.Web.Servicios;

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

    /// <summary>Obtiene el nombre y los roles de la sesión cuando se crea el diseño de la página.</summary>
    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) return;
        usuario = await Autenticacion.ObtenerUsuarioAsync();
        var roles = await Autenticacion.ObtenerRolesAsync();
        esAdministrador = roles.Contains(RolesPantalla.Administrador, StringComparer.OrdinalIgnoreCase);
        esStock = roles.Contains(RolesPantalla.Stock, StringComparer.OrdinalIgnoreCase);
        esOperacion = esAdministrador
            || roles.Contains(RolesPantalla.Supervisor, StringComparer.OrdinalIgnoreCase)
            || roles.Contains(RolesPantalla.Caja, StringComparer.OrdinalIgnoreCase);
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
