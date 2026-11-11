using System.Reflection;
using System.Security.Claims;
using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Controllers;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class Sprint2ReglasTests
{
    [Fact]
    public void Rn008_precio_es_base_mas_adicionales()
    {
        var total = ReglasNegocio.CalcularPrecioLavado(50000, [10000, 5000]);
        Assert.Equal(65000m, total);
    }

    [Fact]
    public void Rn008_rechaza_precios_negativos_o_con_decimales()
    {
        Assert.Throws<ReglaNegocioException>(() => ReglasNegocio.CalcularPrecioLavado(-1, []));
        Assert.Throws<ReglaNegocioException>(() => ReglasNegocio.CalcularPrecioLavado(1000, [1.5m]));
    }

    [Fact]
    public void Rn014_consolida_el_consumo_configurado_del_mismo_insumo()
    {
        var consumo = ReglasNegocio.ConsolidarConsumo([(3, 2m), (3, 1m), (8, 4m)]);
        Assert.Equal([(3, 3m), (8, 4m)], consumo);
    }

    [Fact]
    public void Rn014_el_consumo_reduce_el_stock_y_no_lo_deja_negativo()
    {
        var consumo = ReglasNegocio.ConsolidarConsumo([(1, 2m)]);
        Assert.Equal(8m, ReglasNegocio.AplicarMovimiento(10, ReglasNegocio.Salida, consumo[0].Cantidad));
        Assert.Throws<ReglaNegocioException>(() => ReglasNegocio.AplicarMovimiento(1, ReglasNegocio.Salida, 2));
    }

    [Fact]
    public void Compra_suma_stock_y_anularla_lo_revierte()
    {
        var ingresado = ReglasNegocio.AplicarMovimiento(5, ReglasNegocio.Entrada, 10);
        Assert.Equal(15m, ingresado);
        Assert.Equal(5m, ReglasNegocio.RevertirEntradaDeCompra(ingresado, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.5)]
    public void Cantidad_invalida_en_movimiento_falla(decimal cantidad) =>
        Assert.Throws<ReglaNegocioException>(() => ReglasNegocio.AplicarMovimiento(10, ReglasNegocio.Entrada, cantidad));

    [Fact]
    public async Task Login_de_usuario_activo_devuelve_token()
    {
        var hasher = new PasswordHasher();
        var usuario = new Usuario { Id_Usuarios = 3, Nombre = "ana", Password = hasher.Hashear("Clave123"), Activo = true, Roles = [new Rol { Descripcion = "Administrador" }] };
        var controller = new AuthController(new UsuarioFijo(usuario), new RolesVacios(), hasher, Configuracion());

        var resultado = await controller.Login(new CredencialesRequest { Nombre = "ana", Password = "Clave123" }, CancellationToken.None);

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public async Task Ca31_y_ca33_el_administrador_desactiva_y_vuelve_a_activar()
    {
        var usuario = new Usuario { Id_Usuarios = 4, Nombre = "caja", Activo = true };
        var repo = new UsuarioMutable(usuario);
        var controller = Controlador(repo, idAutenticado: 1);

        var desactivado = await controller.CambiarEstado(4, new EstadoUsuarioRequest(false), CancellationToken.None);
        Assert.False(usuario.Activo);
        Assert.IsType<OkObjectResult>(desactivado);

        var activado = await controller.CambiarEstado(4, new EstadoUsuarioRequest(true), CancellationToken.None);
        Assert.True(usuario.Activo);
        Assert.IsType<OkObjectResult>(activado);
    }

    [Fact]
    public async Task Ca34_el_administrador_no_puede_desactivar_su_propia_cuenta()
    {
        var usuario = new Usuario { Id_Usuarios = 1, Nombre = "admin", Activo = true };
        var controller = Controlador(new UsuarioMutable(usuario), idAutenticado: 1);

        var resultado = await controller.CambiarEstado(1, new EstadoUsuarioRequest(false), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.True(usuario.Activo);
    }

    [Fact]
    public void Rn013_solo_el_administrador_crea_usuarios_y_asigna_roles()
    {
        var atributo = typeof(UsuariosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(atributo);
        Assert.Equal(RolesAutoBrillo.Administrador, atributo!.Roles);
        Assert.NotNull(typeof(UsuariosController).GetMethod(nameof(UsuariosController.Crear)));
        Assert.NotNull(typeof(UsuariosController).GetMethod(nameof(UsuariosController.Editar)));
    }

    [Fact]
    public void Anular_compra_exige_rol_administrador()
    {
        var metodo = typeof(ComprasController).GetMethod(nameof(ComprasController.Anular));
        var atributo = metodo!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.Equal(RolesAutoBrillo.Administrador, atributo!.Roles);
    }

    [Fact]
    public async Task Stock_no_puede_anular_una_compra()
    {
        var controller = new ComprasController(new ComprasQueNoDebenLlamarse());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Stock")], "test"))
            }
        };

        var resultado = await controller.Anular(1, new AnulacionRequest("error de carga"), CancellationToken.None);

        Assert.IsType<ForbidResult>(resultado);
    }

    [Fact]
    public void Los_precios_solo_los_modifica_el_administrador()
    {
        var clase = typeof(ServiciosController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.Equal(RolesAutoBrillo.Administrador, clase!.Roles);
        Assert.Equal(RolesAutoBrillo.Administrador, typeof(ServiciosController).GetMethod(nameof(ServiciosController.GuardarPrecio))!.GetCustomAttribute<AuthorizeAttribute>()?.Roles ?? clase.Roles);
    }

    [Theory]
    [InlineData("tipos_vehiculos")]
    [InlineData("precios_servicios")]
    [InlineData("vehiculos")]
    [InlineData("movimientos_stock")]
    [InlineData("compras")]
    [InlineData("compras_detalle")]
    [InlineData("servicios_insumos")]
    public void Las_tablas_nuevas_tienen_rls(string tabla)
    {
        var contenido = File.ReadAllText(Path.Combine(Raiz(), "supabase", "migrations", "20261111070000_habilitar_rls_sprint2.sql"));
        Assert.Contains($"alter table public.{tabla} enable row level security;", contenido);
    }

    [Fact]
    public void Las_migraciones_nuevas_son_aditivas_e_idempotentes()
    {
        var carpeta = Path.Combine(Raiz(), "supabase", "migrations");
        var nuevas = Directory.GetFiles(carpeta, "20261111*.sql");
        Assert.NotEmpty(nuevas);
        var texto = string.Join('\n', nuevas.Select(File.ReadAllText)).ToLowerInvariant();
        Assert.Contains("create table if not exists", texto);
        Assert.DoesNotContain("drop table", texto);
        Assert.DoesNotContain("drop column", texto);
        Assert.Contains("add column if not exists", texto);
    }

    [Fact]
    public void No_queda_la_migracion_de_ef_que_recrea_usuarios()
    {
        var migraciones = Path.Combine(Raiz(), "src", "AutoBrillo.Repository", "Migrations");
        Assert.False(Directory.Exists(migraciones));
        var csproj = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Api", "AutoBrillo.Api.csproj"));
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.Design", csproj);
    }

    [Theory]
    [InlineData("Compras.razor")]
    [InlineData("Servicios.razor")]
    [InlineData("Vehiculos.razor")]
    public void Las_paginas_usan_pascal_case(string pagina)
    {
        Assert.True(File.Exists(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", pagina)));
        Assert.False(File.Exists(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", pagina.ToLowerInvariant())));
    }

    private static UsuariosController Controlador(IUserRepository usuarios, int idAutenticado)
    {
        var controller = new UsuariosController(usuarios, new RolesVacios(), new PasswordHasher(), new PersonasVacias());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, idAutenticado.ToString())], "test"))
            }
        };
        return controller;
    }

    private static IConfiguration Configuracion() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JWT_SECRET"] = "clave-de-prueba-segura-para-jwt-123456"
    }).Build();

    private static string Raiz()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null && !File.Exists(Path.Combine(carpeta.FullName, "AutoBrillo.slnx"))) carpeta = carpeta.Parent;
        return carpeta!.FullName;
    }

    private sealed class UsuarioFijo(Usuario usuario) : IUserRepository
    {
        public Task<Usuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(usuario);
        public Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Usuario>());
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(usuario);
        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> PersonaYaVinculadaAsync(int idPersona, int? excluirUsuarioId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Usuario entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Usuario entidad) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class UsuarioMutable(Usuario usuario) : IUserRepository
    {
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(id == usuario.Id_Usuarios ? usuario : null);
        public Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Usuario> { usuario });
        public Task<Usuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> PersonaYaVinculadaAsync(int idPersona, int? excluirUsuarioId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Usuario entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Usuario entidad) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RolesVacios : IRolRepository
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

    private sealed class PersonasVacias : IPersonaRepository
    {
        public Task<List<Persona>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Persona>());
        public Task<Persona?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Persona?>(null);
        public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Persona persona, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Persona persona) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ComprasQueNoDebenLlamarse : IRepositorioCompras
    {
        public Task<List<Compra>> ObtenerTodasAsync(CancellationToken ct = default) => throw new InvalidOperationException();
        public Task<Compra> RegistrarAsync(Compra compra, int idUsuario, CancellationToken ct = default) => throw new InvalidOperationException();
        public Task AnularAsync(int id, string motivo, int idUsuario, CancellationToken ct = default) => throw new InvalidOperationException();
    }
}
