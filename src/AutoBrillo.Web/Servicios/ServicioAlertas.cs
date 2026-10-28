using Microsoft.JSInterop;

namespace AutoBrillo.Web.Servicios;

/// <summary>Centraliza confirmaciones y resultados visuales de eliminación.</summary>
public class ServicioAlertas(IJSRuntime javascript)
{
    public ValueTask<bool> ConfirmarCambioEstadoAsync(string tipoRegistro, string accion, string nombre)
        => javascript.InvokeAsync<bool>(
            "autobrilloAlerta.confirmarEliminar",
            $"¿{accion[..1].ToUpper()}{accion[1..]} {tipoRegistro}?",
            nombre);

    public ValueTask<bool> ConfirmarCambioEstadoUsuarioAsync(string accion, string nombre)
        => ConfirmarCambioEstadoAsync("usuario", accion, nombre);

    public ValueTask<bool> ConfirmarEliminarUsuarioAsync(string nombre)
        => javascript.InvokeAsync<bool>(
            "autobrilloAlerta.confirmarEliminar",
            "¿Eliminar usuario?",
            nombre);

    public ValueTask<bool> ConfirmarEliminarRolAsync(string descripcion)
        => javascript.InvokeAsync<bool>(
            "autobrilloAlerta.confirmarEliminarRol",
            "¿Eliminar rol?",
            descripcion);

    public ValueTask<bool> ConfirmarAgregarRolAsync(string descripcion)
        => javascript.InvokeAsync<bool>(
            "autobrilloAlerta.confirmarAgregarRol",
            "¿Agregar rol?",
            descripcion);

    public ValueTask MostrarErrorGeneralAsync(string titulo, string detalle)
        => javascript.InvokeVoidAsync("autobrilloAlerta.mensajeError", titulo, detalle);

    public ValueTask MostrarEliminacionExitosaAsync(string mensaje)
        => javascript.InvokeVoidAsync("autobrilloAlerta.exito", mensaje);

    public ValueTask MostrarErrorEliminarAsync(string detalle)
        => javascript.InvokeVoidAsync("autobrilloAlerta.error", detalle);
}
