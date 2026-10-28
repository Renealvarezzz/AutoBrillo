using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

public partial class Personas
{
    private bool cargando = true;
    private List<PersonaDto> personas = [];
    private List<TipoPersonaDto> tipos = [];
    private PersonaEdicionDto edicion = new();
    private TipoPersonaDto tipoEdicion = new();

    protected override async Task OnInitializedAsync()
    {
        if (!await Autenticacion.ValidarSesionAsync()) { Navegacion.NavigateTo("login", true); return; }
        if (!await Autenticacion.EsAdministradorAsync()) { Navegacion.NavigateTo("inicio", true); return; }
        await CargarAsync();
        cargando = false;
    }

    private async Task CargarAsync()
    {
        personas = await Gestion.ObtenerPersonasAsync() ?? [];
        tipos = await Gestion.ObtenerTiposPersonasAsync() ?? [];
    }

    private bool TipoSeleccionado(int id) => edicion.Id_Tipos_Personas.Contains(id);
    private void CambiarTipo(int id)
    {
        if (TipoSeleccionado(id)) edicion.Id_Tipos_Personas.Remove(id);
        else edicion.Id_Tipos_Personas.Add(id);
    }

    private async Task GuardarPersona()
    {
        if (string.IsNullOrWhiteSpace(edicion.Nombre) || string.IsNullOrWhiteSpace(edicion.Documento) || edicion.Id_Tipos_Personas.Count == 0)
        {
            await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Completá nombre, documento y al menos un tipo.");
            return;
        }
        var error = await Gestion.GuardarPersonaAsync(edicion);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo guardar la persona", error); return; }
        CancelarPersona();
        await CargarAsync();
    }

    private void EditarPersona(PersonaDto persona) => edicion = new PersonaEdicionDto
    {
        Id_Personas = persona.Id_Personas, Nombre = persona.Nombre, Documento = persona.Documento,
        Telefono = persona.Telefono, Id_Tipos_Personas = [.. persona.Id_Tipos_Personas]
    };
    private void CancelarPersona() => edicion = new();

    private async Task CambiarEstado(PersonaDto persona)
    {
        var nuevoEstado = !persona.Activo;
        var accion = nuevoEstado ? "activar" : "desactivar";
        if (!await Alertas.ConfirmarCambioEstadoAsync("persona", accion, persona.Nombre)) return;
        var error = await Gestion.CambiarEstadoPersonaAsync(persona.Id_Personas, nuevoEstado);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo cambiar el estado", error); return; }
        await CargarAsync();
    }

    private async Task EliminarPersona(PersonaDto persona)
    {
        if (!await Alertas.ConfirmarEliminarUsuarioAsync(persona.Nombre)) return;
        var error = await Gestion.EliminarPersonaAsync(persona.Id_Personas);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo eliminar la persona", error); return; }
        await CargarAsync();
    }

    private async Task GuardarTipo()
    {
        if (string.IsNullOrWhiteSpace(tipoEdicion.Descripcion)) { await Alertas.MostrarErrorGeneralAsync("Revisá los datos", "Escribí la descripción del tipo."); return; }
        var error = await Gestion.GuardarTipoPersonaAsync(tipoEdicion);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo guardar el tipo", error); return; }
        tipoEdicion = new();
        await CargarAsync();
    }

    private void EditarTipo(TipoPersonaDto tipo) => tipoEdicion = new TipoPersonaDto { Id_Tipos_Personas = tipo.Id_Tipos_Personas, Descripcion = tipo.Descripcion };
    private async Task EliminarTipo(TipoPersonaDto tipo)
    {
        if (!await Alertas.ConfirmarEliminarRolAsync(tipo.Descripcion)) return;
        var error = await Gestion.EliminarTipoPersonaAsync(tipo.Id_Tipos_Personas);
        if (error is not null) { await Alertas.MostrarErrorGeneralAsync("No se pudo eliminar el tipo", error); return; }
        if (tipoEdicion.Id_Tipos_Personas == tipo.Id_Tipos_Personas) tipoEdicion = new();
        await CargarAsync();
    }
}
