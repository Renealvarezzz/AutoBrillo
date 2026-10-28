using System.Text.Json;
using AutoBrillo.Api.Controllers;
using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class RolesApiTests
{
    [Fact]
    public async Task ObtenerTodos_serializa_roles_sin_ciclos_de_navegacion()
    {
        var rol = new Rol { Id_Roles = 1, Descripcion = "Administrador" };
        var usuario = new Usuario { Id_Usuarios = 7, Nombre = "ana" };
        rol.Usuarios.Add(usuario);
        usuario.Roles.Add(rol);
        var controller = new RolesController(new RolesConNavegacion(rol));

        var resultado = await controller.ObtenerTodos(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);

        var json = JsonSerializer.Serialize(ok.Value);
        Assert.DoesNotContain("Usuarios", json);
        Assert.Contains("Administrador", json);
    }

    private sealed class RolesConNavegacion(Rol rol) : IRolRepository
    {
        public Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Rol> { rol });
        public Task<Rol?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Rol?>(null);
        public Task<Rol?> ObtenerPorDescripcionAsync(string descripcion, CancellationToken cancellationToken = default) => Task.FromResult<Rol?>(null);
        public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> TieneUsuariosAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Rol rol, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Rol rol) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
