using AutoBrillo.Model.Entidades;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class ProductoModeloTests
{
    [Fact]
    public void Producto_nuevo_debe_iniciar_activo_y_permitir_varios_tipos()
    {
        var producto = new Producto();
        producto.Tipos.Add(new TipoProducto { Id_Tipos_Productos = 1, Descripcion = "Servicio" });
        producto.Tipos.Add(new TipoProducto { Id_Tipos_Productos = 2, Descripcion = "Insumo" });

        Assert.True(producto.Activo);
        Assert.Equal(2, producto.Tipos.Count);
    }
}
