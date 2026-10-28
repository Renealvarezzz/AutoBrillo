using Xunit;

namespace AutoBrillo.Web.Tests;

public class SeguridadRlsTests
{
    [Theory]
    [InlineData("personas")]
    [InlineData("personas_tipos")]
    [InlineData("productos")]
    [InlineData("productos_tipos")]
    [InlineData("tipos_personas")]
    [InlineData("tipos_productos")]
    public void Las_tablas_administrativas_expuestas_tienen_RLS_habilitado(string tabla)
    {
        var rutaMigracion = Path.Combine(
            EncontrarProyecto(),
            "supabase",
            "migrations",
            "20261027010000_habilitar_rls_tablas_administrativas.sql");

        var contenido = File.ReadAllText(rutaMigracion);

        Assert.Contains($"alter table public.{tabla} enable row level security;", contenido);
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
