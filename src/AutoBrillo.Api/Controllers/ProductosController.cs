using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Api.Validaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/productos")]
public class ProductosController(IProductoRepository productos, ITipoProductoRepository tipos) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductoResponse>>> ObtenerTodos(CancellationToken cancellationToken) => Ok((await productos.ObtenerTodosAsync(cancellationToken)).Select(Convertir).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(ProductoRequest request, CancellationToken cancellationToken)
    {
        var validacion = await ValidarAsync(request, null, cancellationToken);
        if (validacion.Error is not null) return validacion.Error;
        var producto = new Producto { Descripcion = request.Descripcion.Trim(), PrecioDeCosto = request.PrecioDeCosto, PrecioDeVenta = request.PrecioDeVenta, StockActual = request.StockActual, Tipos = validacion.Tipos! };
        await productos.AgregarAsync(producto, cancellationToken);
        await productos.GuardarCambiosAsync(cancellationToken);
        producto = await productos.ObtenerPorIdAsync(producto.Id_Productos, cancellationToken) ?? producto;
        return CreatedAtAction(nameof(ObtenerTodos), new { producto.Id_Productos }, Convertir(producto));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, ProductoRequest request, CancellationToken cancellationToken)
    {
        var producto = await productos.ObtenerPorIdAsync(id, cancellationToken);
        if (producto is null) return NotFound(new { mensaje = "Producto no encontrado." });
        var validacion = await ValidarAsync(request, id, cancellationToken);
        if (validacion.Error is not null) return validacion.Error;
        producto.Descripcion = request.Descripcion.Trim();
        producto.PrecioDeCosto = request.PrecioDeCosto;
        producto.PrecioDeVenta = request.PrecioDeVenta;
        producto.StockActual = request.StockActual;
        producto.Tipos = validacion.Tipos!;
        await productos.GuardarCambiosAsync(cancellationToken);
        producto = await productos.ObtenerPorIdAsync(id, cancellationToken) ?? producto;
        return Ok(Convertir(producto));
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoUsuarioRequest request, CancellationToken cancellationToken)
    {
        var producto = await productos.ObtenerPorIdAsync(id, cancellationToken);
        if (producto is null) return NotFound(new { mensaje = "Producto no encontrado." });
        producto.Activo = request.Activo;
        await productos.GuardarCambiosAsync(cancellationToken);
        return Ok(Convertir(producto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var producto = await productos.ObtenerPorIdAsync(id, cancellationToken);
        if (producto is null) return NotFound(new { mensaje = "Producto no encontrado." });
        productos.Eliminar(producto);
        await productos.GuardarCambiosAsync(cancellationToken);
        return NoContent();
    }

    private async Task<(IActionResult? Error, List<TipoProducto>? Tipos)> ValidarAsync(ProductoRequest request, int? excluirId, CancellationToken cancellationToken)
    {
        var descripcion = request.Descripcion?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(descripcion)) return (BadRequest(new { mensaje = "La descripción es obligatoria." }), null);
        if (request.PrecioDeCosto < 0 || request.PrecioDeVenta < 0 || request.StockActual < 0) return (BadRequest(new { mensaje = "Precios y stock no pueden ser negativos." }), null);
        if (!ValidadorProducto.TieneValoresEnteros(request.PrecioDeCosto, request.PrecioDeVenta, request.StockActual)) return (BadRequest(new { mensaje = "Precios y stock deben ser números enteros en guaraníes." }), null);
        if (await productos.ExisteDescripcionAsync(descripcion, excluirId, cancellationToken)) return (Conflict(new { mensaje = "Ya existe un producto con esa descripción." }), null);
        var ids = request.Id_Tipos_Productos.Distinct().ToList();
        if (ids.Count == 0) return (BadRequest(new { mensaje = "Seleccioná al menos un tipo de producto." }), null);
        var seleccionados = new List<TipoProducto>();
        foreach (var id in ids)
        {
            var tipo = await tipos.ObtenerPorIdAsync(id, cancellationToken);
            if (tipo is null) return (BadRequest(new { mensaje = "Hay un tipo de producto inválido." }), null);
            seleccionados.Add(tipo);
        }
        return (null, seleccionados);
    }

    private static ProductoResponse Convertir(Producto producto) => new(producto.Id_Productos, producto.Descripcion, producto.PrecioDeCosto, producto.PrecioDeVenta, producto.Activo, producto.StockActual, producto.Tipos.Select(x => x.Id_Tipos_Productos).ToList(), producto.Tipos.OrderBy(x => x.Descripcion).Select(x => x.Descripcion).ToList());
}
