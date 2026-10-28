using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

/// <summary>Página del ABM de Roles. La base se consulta exclusivamente mediante AutoBrillo.Api.</summary>
public partial class Roles
{
    private bool cargando = true;
    private List<RolDto> roles = [];
    private RolDto edicion = new();

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        if (!await Autenticacion.EsAdministradorAsync()) { Navegacion.NavigateTo("inicio", true); return; }
        await CargarAsync();
        cargando = false;
    }

    private async Task CargarAsync() => roles = await Gestion.ObtenerRolesAsync() ?? [];

    private async Task Guardar()
    {
        if (string.IsNullOrWhiteSpace(edicion.Descripcion))
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Escribí la descripción del rol.");
            return;
        }

        if (edicion.Id_Roles == 0 && !await Alertas.ConfirmarAgregarRolAsync(edicion.Descripcion))
        {
            return;
        }

        var error = await Gestion.GuardarRolAsync(edicion);
        if (error is not null)
        {
            await Alertas.MostrarErrorGeneralAsync("No se pudo guardar el rol", error);
            return;
        }

        Cancelar();
        await CargarAsync();
    }

    private void Editar(RolDto rol) => edicion = new RolDto { Id_Roles = rol.Id_Roles, Descripcion = rol.Descripcion };
    private void Cancelar() => edicion = new();

    private async Task Eliminar(RolDto rol)
    {
        if (!await Alertas.ConfirmarEliminarRolAsync(rol.Descripcion)) return;

        var error = await Gestion.EliminarRolAsync(rol.Id_Roles);
        if (error is not null)
        {
            await Alertas.MostrarErrorGeneralAsync("No se pudo eliminar el rol", error);
            return;
        }

        await CargarAsync();
        await Alertas.MostrarEliminacionExitosaAsync($"El rol {rol.Descripcion} fue eliminado correctamente.");
    }
}
