using Xunit;

namespace AutoBrillo.Web.Tests;

public class MetadatosWhatsAppTests
{
    [Fact]
    public void El_indice_publica_metadatos_Open_Graph_para_explicar_AutoBrillo_en_WhatsApp()
    {
        var rutaProyecto = EncontrarProyecto();
        var contenido = File.ReadAllText(Path.Combine(rutaProyecto, "src", "AutoBrillo.Web", "wwwroot", "index.html"));

        Assert.Contains("<meta name=\"description\" content=\"Sistema para gestionar usuarios, roles y operaciones del lavadero.\" />", contenido);
        Assert.Contains("<meta property=\"og:title\" content=\"AutoBrillo\" />", contenido);
        Assert.Contains("<meta property=\"og:description\" content=\"Sistema para gestionar usuarios, roles y operaciones del lavadero.\" />", contenido);
        Assert.Contains("<meta property=\"og:url\" content=\"https://autobrillo-web.vercel.app/\" />", contenido);
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
