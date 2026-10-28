using AutoBrillo.Model.Entidades;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class UsuarioEstadoTests
{
    [Fact]
    public void Usuario_nuevo_debe_estar_activo_por_defecto()
    {
        var usuario = new Usuario();

        var propiedad = typeof(Usuario).GetProperty("Activo");

        Assert.NotNull(propiedad);
        Assert.True((bool)propiedad!.GetValue(usuario)!);
    }
}
