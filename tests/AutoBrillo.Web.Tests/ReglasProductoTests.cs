using AutoBrillo.Api.Validaciones;
using AutoBrillo.Web.Servicios;
using Microsoft.JSInterop;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class ReglasProductoTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(25000, 50000, 3)]
    public void Los_precios_y_stock_enteros_son_validos(decimal costo, decimal venta, decimal stock)
    {
        Assert.True(ValidadorProducto.TieneValoresEnteros(costo, venta, stock));
    }

    [Theory]
    [InlineData(25000.50, 50000, 0)]
    [InlineData(25000, 50000.25, 0)]
    [InlineData(25000, 50000, 1.5)]
    public void Los_precios_y_stock_con_decimales_son_invalidos(decimal costo, decimal venta, decimal stock)
    {
        Assert.False(ValidadorProducto.TieneValoresEnteros(costo, venta, stock));
    }

    [Fact]
    public async Task ConfirmarCambioEstado_muestra_tipo_accion_y_registro()
    {
        var javascript = new JavaScriptFalso(true);
        var alertas = new ServicioAlertas(javascript);

        var confirmado = await alertas.ConfirmarCambioEstadoAsync("producto", "desactivar", "Lavado completo");

        Assert.True(confirmado);
        Assert.Equal("autobrilloAlerta.confirmarEliminar", javascript.Identificador);
        Assert.Equal("¿Desactivar producto?", javascript.Argumentos[0]);
        Assert.Equal("Lavado completo", javascript.Argumentos[1]);
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
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
