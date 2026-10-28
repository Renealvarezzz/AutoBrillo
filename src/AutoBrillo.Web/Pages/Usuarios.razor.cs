using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

/// <summary>Página del ABM de Usuarios. Las contraseñas viajan a la API y allí se almacenan como BCrypt.</summary>
public partial class Usuarios
{
    private bool cargando = true;
    private List<UsuarioDto> usuarios = [];
    private List<RolDto> roles = [];
    private UsuarioEdicionDto edicion = new();
    private string confirmacionPassword = string.Empty;
    private bool mostrarPasswords;

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        if (!await Autenticacion.EsAdministradorAsync()) { Navegacion.NavigateTo("inicio", true); return; }
        await CargarAsync();
        cargando = false;
    }

    private async Task CargarAsync()
    {
        usuarios = await Gestion.ObtenerUsuariosAsync() ?? [];
        roles = await Gestion.ObtenerRolesAsync() ?? [];
    }

    private async Task Guardar()
    {
        if (string.IsNullOrWhiteSpace(edicion.Nombre) || edicion.Id_Roles.Count == 0 || (edicion.Id_Usuarios == 0 && string.IsNullOrWhiteSpace(edicion.Password)))
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Completá nombre, contraseña y al menos un rol.");
            return;
        }

        var modificoPassword = edicion.Id_Usuarios == 0 || !string.IsNullOrWhiteSpace(edicion.Password) || !string.IsNullOrWhiteSpace(confirmacionPassword);
        if (modificoPassword && !ValidadorPasswordUsuario.Coinciden(edicion.Password, confirmacionPassword))
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Las contraseñas no coinciden. Revisalas e intentá de nuevo.");
            return;
        }

        var error = await Gestion.GuardarUsuarioAsync(edicion);
        if (error is not null)
        {
            await Alertas.MostrarErrorGeneralAsync("No se pudo guardar el usuario", error);
            return;
        }

        Cancelar();
        await CargarAsync();
    }

    private void Editar(UsuarioDto usuario)
    {
        edicion = new UsuarioEdicionDto { Id_Usuarios = usuario.Id_Usuarios, Nombre = usuario.Nombre, Id_Roles = [.. usuario.Id_Roles] };
        confirmacionPassword = string.Empty;
        mostrarPasswords = false;
    }

    private bool EstaSeleccionado(int idRol) => edicion.Id_Roles.Contains(idRol);

    private void CambiarRol(int idRol)
    {
        if (EstaSeleccionado(idRol)) edicion.Id_Roles.Remove(idRol);
        else edicion.Id_Roles.Add(idRol);
    }

    private void Cancelar()
    {
        edicion = new();
        confirmacionPassword = string.Empty;
        mostrarPasswords = false;
    }

    private async Task CambiarEstado(UsuarioDto usuario)
    {
        var nuevoEstado = !usuario.Activo;
        var accion = nuevoEstado ? "activar" : "desactivar";
        if (!await Alertas.ConfirmarCambioEstadoUsuarioAsync(accion, usuario.Nombre)) return;

        var error = await Gestion.CambiarEstadoUsuarioAsync(usuario.Id_Usuarios, nuevoEstado);
        if (error is not null)
        {
            await Alertas.MostrarErrorGeneralAsync($"No se pudo {accion} el usuario", error);
            return;
        }

        await CargarAsync();
    }

    private async Task Eliminar(UsuarioDto usuario)
    {
        if (!await Alertas.ConfirmarEliminarUsuarioAsync(usuario.Nombre)) return;

        var error = await Gestion.EliminarUsuarioAsync(usuario.Id_Usuarios);
        if (error is not null)
        {
            await Alertas.MostrarErrorEliminarAsync(error);
            return;
        }

        await CargarAsync();
        await Alertas.MostrarEliminacionExitosaAsync($"El usuario {usuario.Nombre} fue eliminado correctamente.");
    }
}
