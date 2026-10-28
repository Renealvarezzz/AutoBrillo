using AutoBrillo.Model.Entidades;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class PersonaModeloTests
{
    [Fact]
    public void Persona_nueva_debe_iniciar_activa_y_permitir_varios_tipos()
    {
        var persona = new Persona();
        var cliente = new TipoPersona { Id_Tipos_Personas = 1, Descripcion = "Cliente" };
        var proveedor = new TipoPersona { Id_Tipos_Personas = 2, Descripcion = "Proveedor" };

        persona.Tipos.Add(cliente);
        persona.Tipos.Add(proveedor);

        Assert.True(persona.Activo);
        Assert.Equal(2, persona.Tipos.Count);
    }
}
