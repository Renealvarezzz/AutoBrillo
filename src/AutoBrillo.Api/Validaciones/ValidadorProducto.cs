namespace AutoBrillo.Api.Validaciones;

public static class ValidadorProducto
{
    public static bool TieneValoresEnteros(decimal precioDeCosto, decimal precioDeVenta, decimal stockActual) =>
        decimal.Truncate(precioDeCosto) == precioDeCosto &&
        decimal.Truncate(precioDeVenta) == precioDeVenta &&
        decimal.Truncate(stockActual) == stockActual;
}
