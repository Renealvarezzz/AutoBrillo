namespace AutoBrillo.Model.Reglas;

/// <summary>Error de una regla de negocio; la API lo devuelve como 400 con el mensaje.</summary>
public class ReglaNegocioException(string mensaje) : Exception(mensaje);

/// <summary>
/// Reglas puras del sprint 2. No consultan la base: compras, precios y consumo de insumos
/// las usan y los tests las cubren sin PostgreSQL.
/// </summary>
public static class ReglasNegocio
{
    public const string Entrada = "Entrada";
    public const string Salida = "Salida";

    /// <summary>RN-008: el precio del lavado es el del tipo de vehículo más los adicionales elegidos.</summary>
    public static decimal CalcularPrecioLavado(decimal precioBaseTipoVehiculo, IEnumerable<decimal> preciosAdicionales)
    {
        ValidarImporte(precioBaseTipoVehiculo, "El precio base");
        var total = precioBaseTipoVehiculo;
        foreach (var precio in preciosAdicionales)
        {
            ValidarImporte(precio, "El precio de un adicional");
            total += precio;
        }

        return total;
    }

    /// <summary>
    /// RN-014: junta las cantidades configuradas del mismo insumo.
    /// El stock se descuenta después con <see cref="AplicarMovimiento"/> por cada insumo.
    /// </summary>
    public static List<(int IdInsumo, decimal Cantidad)> ConsolidarConsumo(IEnumerable<(int IdInsumo, decimal Cantidad)> lineas)
    {
        var grupos = new Dictionary<int, decimal>();
        foreach (var (idInsumo, cantidad) in lineas)
        {
            if (idInsumo <= 0) throw new ReglaNegocioException("El insumo configurado no es válido.");
            ValidarCantidad(cantidad);
            grupos[idInsumo] = grupos.GetValueOrDefault(idInsumo) + cantidad;
        }

        return grupos.Select(x => (x.Key, x.Value)).ToList();
    }

    /// <summary>Devuelve el stock resultante de una entrada o salida. No permite stock negativo ni fracciones.</summary>
    public static decimal AplicarMovimiento(decimal stockActual, string tipo, decimal cantidad)
    {
        ValidarCantidad(cantidad);
        var resultado = tipo switch
        {
            Entrada => stockActual + cantidad,
            Salida => stockActual - cantidad,
            _ => throw new ReglaNegocioException("El tipo de movimiento debe ser Entrada o Salida.")
        };
        if (resultado < 0) throw new ReglaNegocioException($"Stock insuficiente: hay {stockActual:N0} y se pidieron {cantidad:N0}.");
        return resultado;
    }

    /// <summary>Una compra anulada saca del stock la misma cantidad que había ingresado.</summary>
    public static decimal RevertirEntradaDeCompra(decimal stockActual, decimal cantidadComprada) =>
        AplicarMovimiento(stockActual, Salida, cantidadComprada);

    private static void ValidarCantidad(decimal cantidad)
    {
        if (cantidad <= 0 || cantidad != decimal.Truncate(cantidad))
            throw new ReglaNegocioException("La cantidad debe ser un número entero mayor a cero.");
    }

    private static void ValidarImporte(decimal importe, string nombre)
    {
        if (importe < 0 || importe != decimal.Truncate(importe))
            throw new ReglaNegocioException($"{nombre} debe ser un entero no negativo.");
    }
}
