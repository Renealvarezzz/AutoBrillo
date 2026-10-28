using Xunit;

namespace AutoBrillo.Web.Tests;

public class BotonesAccionMarkupTests
{
    [Theory]
    [InlineData("Usuarios.razor", "Editar usuario", "Eliminar usuario")]
    [InlineData("Roles.razor", "Editar rol", "Eliminar rol")]
    public void Las_acciones_de_tabla_usan_iconos_con_etiquetas_accesibles(
        string pagina,
        string etiquetaEditar,
        string etiquetaEliminar)
    {
        var rutaProyecto = EncontrarProyecto();
        var contenido = File.ReadAllText(Path.Combine(rutaProyecto, "src", "AutoBrillo.Web", "Pages", pagina));

        Assert.Contains("class=\"boton-icono secundario\"", contenido);
        Assert.Contains("class=\"boton-icono peligro\"", contenido);
        Assert.Contains($"aria-label=\"{etiquetaEditar}\"", contenido);
        Assert.Contains($"aria-label=\"{etiquetaEliminar}\"", contenido);
        Assert.DoesNotContain(">Editar</button>", contenido);
        Assert.DoesNotContain(">Eliminar</button>", contenido);
    }

    private static string EncontrarProyecto()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null)
        {
            if (File.Exists(Path.Combine(carpeta.FullName, "AutoBrillo.slnx")))
            {
                return carpeta.FullName;
            }

            carpeta = carpeta.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró el proyecto AutoBrillo.");
    }
}
