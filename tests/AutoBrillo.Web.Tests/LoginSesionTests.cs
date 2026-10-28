using Xunit;

namespace AutoBrillo.Web.Tests;

public class LoginSesionTests
{
    [Fact]
    public void La_pagina_login_valida_la_sesion_y_redirige_a_inicio_si_ya_existe()
    {
        var contenido = File.ReadAllText(Path.Combine(EncontrarProyecto(), "src", "AutoBrillo.Web", "Pages", "Login.razor.cs"));

        Assert.Contains("OnInitializedAsync", contenido);
        Assert.Contains("Autenticacion.ValidarSesionAsync()", contenido);
        Assert.Contains("Navegacion.NavigateTo(\"inicio\", forceLoad: true)", contenido);
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
