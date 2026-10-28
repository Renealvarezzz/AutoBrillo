using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Controllers;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class AccesoUsuarioTests
{
    [Fact]
    public async Task Login_rechaza_un_usuario_desactivado_aun_con_password_correcta()
    {
        var hasher = new PasswordHasher();
        var usuario = new Usuario
        {
            Id_Usuarios = 7,
            Nombre = "operador",
            Password = hasher.Hashear("Clave123"),
            Activo = false
        };
        var configuracion = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JWT_SECRET"] = "clave-de-prueba-segura-para-jwt-123456",
        }).Build();
        var controller = new AuthController(new UsuarioInactivoRepository(usuario), new RolesNoUsados(), hasher, configuracion);

        var resultado = await controller.Login(new CredencialesRequest { Nombre = "operador", Password = "Clave123" }, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(resultado);
    }

    private sealed class UsuarioInactivoRepository(Usuario usuario) : IUserRepository
    {
        public Task<Usuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default) =>
            Task.FromResult<Usuario?>(nombre == usuario.Nombre ? usuario : null);
        public Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Usuario>());
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Usuario entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Usuario entidad) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RolesNoUsados : IRolRepository
    {
        public Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Rol>());
        public Task<Rol?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Rol?>(null);
        public Task<Rol?> ObtenerPorDescripcionAsync(string descripcion, CancellationToken cancellationToken = default) => Task.FromResult<Rol?>(null);
        public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> TieneUsuariosAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Rol rol, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Rol rol) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
