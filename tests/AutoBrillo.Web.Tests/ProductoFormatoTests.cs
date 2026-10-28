using Xunit;

namespace AutoBrillo.Web.Tests;

public class ProductoFormatoTests
{
    [Fact]
    public void La_lista_de_productos_muestra_importes_y_stock_sin_decimales()
    {
        var contenido = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Pages", "Productos.razor"));

        Assert.Contains("ToString(\"N0\")", contenido);
        Assert.DoesNotContain("ToString(\"N2\")", contenido);
    }

    private static string EncontrarProyecto()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null)
        {
            if (File.Exists(Path.Combine(carpeta.FullName, "AutoBrillo.slnx"))) return carpeta.FullName;
            carpeta = carpeta.Parent;
        }
        throw new DirectoryNotFoundException("No se encontró el proyecto AutoBrillo.");
    }
}
