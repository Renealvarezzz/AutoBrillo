using AutoBrillo.Web.Servicios;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class ValidadorPasswordUsuarioTests
{
    [Fact]
    public void Coinciden_devuelve_falso_si_las_contrasenas_son_distintas()
    {
        Assert.False(ValidadorPasswordUsuario.Coinciden("Clave123", "Clave124"));
    }

    [Fact]
    public void Coinciden_devuelve_verdadero_si_las_contrasenas_son_iguales()
    {
        Assert.True(ValidadorPasswordUsuario.Coinciden("Clave123", "Clave123"));
    }
}
