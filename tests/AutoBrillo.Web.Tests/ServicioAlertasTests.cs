using AutoBrillo.Web.Servicios;
using Microsoft.JSInterop;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class ServicioAlertasTests
{
    [Fact]
    public async Task ConfirmarEliminarUsuario_muestra_dialogo_central_con_el_nombre_del_usuario()
    {
        var javascript = new JavaScriptFalso(true);
        var servicio = new ServicioAlertas(javascript);

        var confirmado = await servicio.ConfirmarEliminarUsuarioAsync("María");

        Assert.True(confirmado);
        Assert.Equal("autobrilloAlerta.confirmarEliminar", javascript.Identificador);
        Assert.Equal("María", javascript.Argumentos[1]);
    }

    [Fact]
    public async Task ConfirmarEliminarRol_muestra_dialogo_central_con_el_nombre_del_rol()
    {
        var javascript = new JavaScriptFalso(true);
        var servicio = new ServicioAlertas(javascript);

        var confirmado = await servicio.ConfirmarEliminarRolAsync("Caja");

        Assert.True(confirmado);
        Assert.Equal("autobrilloAlerta.confirmarEliminarRol", javascript.Identificador);
        Assert.Equal("Caja", javascript.Argumentos[1]);
    }

    [Fact]
    public async Task ConfirmarAgregarRol_muestra_dialogo_central_con_la_descripcion()
    {
        var javascript = new JavaScriptFalso(true);
        var servicio = new ServicioAlertas(javascript);

        var confirmado = await servicio.ConfirmarAgregarRolAsync("Recepción");

        Assert.True(confirmado);
        Assert.Equal("autobrilloAlerta.confirmarAgregarRol", javascript.Identificador);
        Assert.Equal("Recepción", javascript.Argumentos[1]);
    }

    [Fact]
    public async Task MostrarErrorGeneral_muestra_un_mensaje_central_con_titulo_y_detalle()
    {
        var javascript = new JavaScriptFalso(null);
        var servicio = new ServicioAlertas(javascript);

        await servicio.MostrarErrorGeneralAsync("Revisá los datos", "Las contraseñas no coinciden.");

        Assert.Equal("autobrilloAlerta.mensajeError", javascript.Identificador);
        Assert.Equal("Revisá los datos", javascript.Argumentos[0]);
        Assert.Equal("Las contraseñas no coinciden.", javascript.Argumentos[1]);
    }
    [Fact]
    public async Task MostrarEliminacionExitosa_notifica_en_un_dialogo_central()
    {
        var javascript = new JavaScriptFalso(null);
        var servicio = new ServicioAlertas(javascript);

        await servicio.MostrarEliminacionExitosaAsync("María");

        Assert.Equal("autobrilloAlerta.exito", javascript.Identificador);
        Assert.Contains("María", javascript.Argumentos[0]?.ToString());
    }

    [Fact]
    public async Task MostrarEliminacionExitosa_envia_el_mensaje_recibido()
    {
        var javascript = new JavaScriptFalso(null);
        var servicio = new ServicioAlertas(javascript);

        await servicio.MostrarEliminacionExitosaAsync("El rol Caja fue eliminado correctamente.");

        Assert.Equal("autobrilloAlerta.exito", javascript.Identificador);
        Assert.Equal("El rol Caja fue eliminado correctamente.", javascript.Argumentos[0]);
    }
    [Fact]
    public async Task MostrarErrorEliminar_notifica_el_error_en_un_dialogo_central()
    {
        var javascript = new JavaScriptFalso(null);
        var servicio = new ServicioAlertas(javascript);

        await servicio.MostrarErrorEliminarAsync("El usuario no existe.");

        Assert.Equal("autobrilloAlerta.error", javascript.Identificador);
        Assert.Equal("El usuario no existe.", javascript.Argumentos[0]);
    }

    private sealed class JavaScriptFalso(bool? respuesta) : IJSRuntime
    {
        public string? Identificador { get; private set; }
        public object?[] Argumentos { get; private set; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Identificador = identifier;
            Argumentos = args ?? [];
            return ValueTask.FromResult((TValue)(object?)respuesta!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);
    }
}
