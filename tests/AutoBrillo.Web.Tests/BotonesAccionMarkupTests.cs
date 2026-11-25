using Xunit;

namespace AutoBrillo.Web.Tests;

public class BotonesAccionMarkupTests
{
    [Theory]
    [InlineData("Usuarios.razor", "Editar usuario", "Eliminar usuario")]
    [InlineData("Roles.razor", "Editar rol", "Eliminar rol")]
    [InlineData("Productos.razor", "Editar producto", "Eliminar producto")]
    [InlineData("Personas.razor", "Editar persona", "Eliminar persona")]
    public void Las_acciones_de_tabla_son_compactas_y_accesibles(
        string pagina,
        string etiquetaEditar,
        string etiquetaEliminar)
    {
        var contenido = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Pages", pagina));

        Assert.Contains("btn-group btn-group-sm", contenido);
        Assert.Contains("btn btn-sm btn-outline-primary", contenido);
        Assert.Contains("btn btn-sm btn-outline-danger", contenido);
        Assert.Contains($"aria-label=\"{etiquetaEditar}\"", contenido);
        Assert.Contains($"aria-label=\"{etiquetaEliminar}\"", contenido);
        Assert.DoesNotContain(">Editar</button>", contenido);
        Assert.DoesNotContain(">Eliminar</button>", contenido);
        Assert.DoesNotContain("boton-icono", contenido);
    }

    [Fact]
    public void Lavados_usa_el_mismo_grupo_compacto_en_acciones()
    {
        var contenido = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Pages", "Lavados.razor"));

        Assert.Contains("btn-group btn-group-sm", contenido);
        Assert.Contains("aria-label=\"Terminar lavado\"", contenido);
        Assert.Contains("aria-label=\"Editar lavado\"", contenido);
        Assert.Contains("aria-label=\"Cancelar lavado\"", contenido);
        Assert.Contains("class=\"acciones-tabla\"", contenido);
        Assert.DoesNotContain(">Terminar</button>", contenido);
        Assert.DoesNotContain(">Editar</button>", contenido);
        Assert.DoesNotContain(">Cancelar</button>", contenido);
    }

    [Fact]
    public void Los_tipos_se_eligen_con_casillas_visibles()
    {
        var raiz = EncontrarProyecto();
        var productos = File.ReadAllText(Path.Combine(raiz, "src", "AutoBrillo.Web", "Pages", "Productos.razor"));
        var personas = File.ReadAllText(Path.Combine(raiz, "src", "AutoBrillo.Web", "Pages", "Personas.razor"));

        foreach (var contenido in new[] { productos, personas })
        {
            Assert.Contains("lista-checks", contenido);
            Assert.Contains("form-check-input", contenido);
            Assert.Contains("form-check-label", contenido);
            Assert.Contains("type=\"checkbox\"", contenido);
            Assert.DoesNotContain("<details", contenido);
            Assert.DoesNotContain("desplegable-tipos", contenido);
            Assert.DoesNotContain("<select", contenido);
        }

        Assert.Contains("CambiarTipo(idTipo)", productos);
        Assert.Contains("CambiarTipo(idTipo)", personas);
    }

    [Fact]
    public void Agregar_linea_usa_el_boton_outline_de_Bootstrap()
    {
        var editor = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Shared", "EditorLineas.razor"));
        var ventas = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Pages", "Ventas.razor"));

        Assert.Contains("btn btn-outline-primary", editor);
        Assert.Contains("bi-plus-lg", editor);
        Assert.Contains("Agregar línea", editor);
        Assert.DoesNotContain("boton-ancho", editor);
        Assert.DoesNotContain("secundario", editor);
        Assert.Contains("btn btn-outline-primary", ventas);
        Assert.Contains("Agregar línea", ventas);
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
