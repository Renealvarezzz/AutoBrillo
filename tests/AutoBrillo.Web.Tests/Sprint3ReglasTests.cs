using System.Reflection;
using System.Security.Claims;
using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Controllers;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class Sprint3ReglasTests
{
    [Fact]
    public void La_comision_solo_suma_servicios_y_adicionales()
    {
        var baseComision = ReglasNegocio.SumarBaseComision([45000m, 10000m]);
        var importe = ReglasNegocio.CalcularComision(baseComision, 30m, 2);

        Assert.Equal(8250m, importe);
        Assert.NotEqual(ReglasNegocio.CalcularComision(45000m + 10000m + 8000m, 30m, 2), importe);
    }

    [Theory]
    [InlineData("Registrado", false, true)]
    [InlineData("Terminado", false, true)]
    [InlineData("Cancelado", false, false)]
    [InlineData("Terminado", true, false)]
    [InlineData("Registrado", true, false)]
    public void Un_lavado_se_cobra_si_esta_registrado_o_terminado_y_sin_venta_activa(string estado, bool tieneVenta, bool disponible)
    {
        Assert.Equal(disponible, ReglasNegocio.LavadoDisponibleParaVenta(estado, tieneVenta));
    }

    [Theory]
    [InlineData("abc 123", "ABC123")]
    [InlineData(" ABC  123 ", "ABC123")]
    [InlineData("ab-c 12", "AB-C12")]
    public void La_patente_se_compacta_y_pasa_a_mayusculas(string entrada, string esperada) =>
        Assert.Equal(esperada, ReglasNegocio.NormalizarPatente(entrada));

    [Fact]
    public void Una_fecha_utc_pasa_a_hora_de_paraguay_sin_zona()
    {
        var utc = new DateTime(2026, 1, 15, 18, 30, 0, DateTimeKind.Utc);
        var local = ServicioOperativo.NormalizarFecha(utc);
        var offset = ServicioOperativo.ZonaParaguay.GetUtcOffset(utc);

        Assert.Equal(DateTimeKind.Unspecified, local.Kind);
        Assert.Equal(utc.Add(offset).Hour, local.Hour);
        Assert.Equal(utc.Add(offset).Day, local.Day);
    }

    [Fact]
    public void Una_fecha_sin_zona_se_toma_como_hora_del_lavadero()
    {
        var pared = new DateTime(2026, 1, 15, 9, 0, 0, DateTimeKind.Unspecified);
        var normalizada = ServicioOperativo.NormalizarFecha(pared);

        Assert.Equal(pared, normalizada);
        Assert.Equal(DateTimeKind.Unspecified, normalizada.Kind);
    }

    [Fact]
    public void Ahora_no_depende_de_la_zona_del_contenedor()
    {
        var ahora = ServicioOperativo.Ahora();
        var esperado = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ServicioOperativo.ZonaParaguay);

        Assert.Equal(DateTimeKind.Unspecified, ahora.Kind);
        Assert.InRange((ahora - DateTime.SpecifyKind(esperado, DateTimeKind.Unspecified)).Duration(), TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void La_fecha_del_lavado_se_normaliza_antes_de_la_consulta()
    {
        var codigo = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Repository", "Repositorios", "RepositorioSprintTres.cs"));
        var registrar = Extraer(codigo, "public async Task<Lavado> RegistrarLavadoAsync");
        var actualizar = Extraer(codigo, "public async Task<Lavado> ActualizarLavadoAsync");
        Assert.True(registrar.IndexOf("NormalizarFecha", StringComparison.Ordinal) < registrar.IndexOf("Ese sector ya está ocupado", StringComparison.Ordinal));
        Assert.True(actualizar.IndexOf("NormalizarFecha", StringComparison.Ordinal) < actualizar.IndexOf("Ese sector ya está ocupado", StringComparison.Ordinal));
    }

    [Fact]
    public void Cancelar_un_lavado_y_su_venta_ocurre_en_una_sola_transaccion()
    {
        var codigo = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Repository", "Repositorios", "RepositorioSprintTres.cs"));
        var anular = Extraer(codigo, "public async Task AnularLavadoAsync");
        Assert.Contains("BeginTransactionAsync", anular);
        Assert.Contains("CommitAsync", anular);
        Assert.DoesNotContain("AnularVentaAsync", anular);
        Assert.Contains("AplicarAnulacionVentaAsync", anular);
    }

    [Fact]
    public void El_precio_enviado_por_el_cliente_no_forma_parte_del_contrato() =>
        Assert.Null(typeof(VentaLineaRequest).GetProperty("PrecioUnitario"));

    [Theory]
    [InlineData(nameof(OperacionesController.RegistrarLavado))]
    [InlineData(nameof(OperacionesController.EditarLavado))]
    [InlineData(nameof(OperacionesController.FinalizarLavado))]
    [InlineData(nameof(OperacionesController.Cobrar))]
    public void Supervisor_y_caja_operan_lavados_ventas_y_cobros(string metodo)
    {
        var roles = typeof(OperacionesController).GetMethod(metodo)!.GetCustomAttribute<AuthorizeAttribute>()!.Roles;
        Assert.Equal(RolesAutoBrillo.Operacion, roles);
        Assert.Contains(RolesAutoBrillo.Supervisor, roles);
        Assert.Contains(RolesAutoBrillo.Caja, roles);
    }

    [Fact]
    public void Solo_el_administrador_anula_lavados_ventas_y_ve_comisiones()
    {
        Assert.Equal(RolesAutoBrillo.Administrador, typeof(OperacionesController).GetMethod(nameof(OperacionesController.CancelarLavado))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles);
        Assert.Equal(RolesAutoBrillo.Administrador, typeof(OperacionesController).GetMethod(nameof(OperacionesController.AnularVenta))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles);
        Assert.Equal(RolesAutoBrillo.Administrador, typeof(OperacionesController).GetMethod(nameof(OperacionesController.Comisiones))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles);
    }

    [Fact]
    public void Stock_puede_leer_productos_y_personas_pero_no_editarlos()
    {
        Assert.Contains(RolesAutoBrillo.Stock, typeof(ProductosController).GetCustomAttribute<AuthorizeAttribute>()!.Roles);
        Assert.Contains(RolesAutoBrillo.Stock, typeof(PersonasController).GetCustomAttribute<AuthorizeAttribute>()!.Roles);
        Assert.Equal("Administrador", typeof(ProductosController).GetMethod(nameof(ProductosController.Editar))!.GetCustomAttribute<AuthorizeAttribute>()!.Roles);
        Assert.Equal("Administrador", typeof(PersonasController).GetMethod("Crear")!.GetCustomAttribute<AuthorizeAttribute>()!.Roles);
    }

    [Fact]
    public void Los_precios_y_el_stock_del_modelo_coinciden_con_numeric_18_0()
    {
        using var ambito = new AmbitoSqlite();
        var producto = ambito.Db.Model.FindEntityType(typeof(Producto))!;
        Assert.Equal(0, producto.FindProperty(nameof(Producto.PrecioDeCosto))!.GetScale());
        Assert.Equal(0, producto.FindProperty(nameof(Producto.PrecioDeVenta))!.GetScale());
        Assert.Equal(0, producto.FindProperty(nameof(Producto.StockActual))!.GetScale());
        var indice = ambito.Db.Model.FindEntityType(typeof(PrecioServicio))!.GetIndexes().Single(x => x.GetDatabaseName() == "ux_precios_servicios_vigente");
        Assert.True(indice.IsUnique);
        Assert.Contains("Vigente", indice.GetFilter());
    }

    [Fact]
    public async Task Editar_un_producto_no_cambia_el_stock()
    {
        var tipo = new TipoProducto { Id_Tipos_Productos = 1, Descripcion = "Insumo" };
        var producto = new Producto { Id_Productos = 3, Descripcion = "Shampoo", PrecioDeCosto = 1000, PrecioDeVenta = 2000, StockActual = 8, Tipos = [tipo] };
        var controller = new ProductosController(new ProductosFijos(producto), new TiposFijos(tipo));

        var resultado = await controller.Editar(3, new ProductoRequest("Shampoo plus", 1000, 2500, 99, [1]), CancellationToken.None);

        Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(8, producto.StockActual);
        Assert.Equal("Shampoo plus", producto.Descripcion);
    }

    [Fact]
    public void No_se_puede_dejar_el_sistema_sin_administrador_activo()
    {
        var admin = Usuario(1, true, "Administrador");
        var caja = Usuario(2, true, "Caja");
        Assert.True(ReglasUsuarios.QuedariaSinAdministradorActivo([admin, caja], 1, true, false));
        Assert.True(ReglasUsuarios.QuedariaSinAdministradorActivo([admin], 1, false, true));
        Assert.False(ReglasUsuarios.QuedariaSinAdministradorActivo([admin, Usuario(3, true, "Administrador")], 1, false, false));
        Assert.False(ReglasUsuarios.QuedariaSinAdministradorActivo([admin, caja], 2, false, false));
    }

    [Fact]
    public async Task Un_administrador_no_puede_quitararse_el_rol_si_es_el_ultimo()
    {
        var admin = Usuario(1, true, "Administrador");
        var repo = new UsuariosEnMemoria(admin);
        var controller = Controlador(repo, 1, new Rol { Id_Roles = 1, Descripcion = "Administrador" }, new Rol { Id_Roles = 2, Descripcion = "Caja" });

        var resultado = await controller.Editar(1, new UsuarioRequest { Nombre = "admin", Id_Roles = [2] }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Contains("Administrador", admin.Roles.Select(x => x.Descripcion));
    }

    [Fact]
    public async Task No_se_puede_eliminar_la_propia_cuenta_ni_al_ultimo_administrador()
    {
        var admin = Usuario(1, true, "Administrador");
        var repo = new UsuariosEnMemoria(admin);
        var controller = Controlador(repo, 1, new Rol { Id_Roles = 1, Descripcion = "Administrador" });

        var propio = await controller.Eliminar(1, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(propio);
        Assert.False(repo.Elimino);

        var ajeno = Controlador(repo, 9, new Rol { Id_Roles = 1, Descripcion = "Administrador" });
        var ultimo = await ajeno.Eliminar(1, CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(ultimo);
        Assert.False(repo.Elimino);
        Assert.True(admin.Activo);
    }

    [Fact]
    public async Task Desactivar_al_ultimo_administrador_esta_bloqueado()
    {
        var admin = Usuario(1, true, "Administrador");
        var controller = Controlador(new UsuariosEnMemoria(admin), 9, new Rol { Id_Roles = 1, Descripcion = "Administrador" });

        var resultado = await controller.CambiarEstado(1, new EstadoUsuarioRequest(false), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.True(admin.Activo);
    }

    [Fact]
    public void Las_pantallas_dejan_de_usar_prompt_y_admiten_varios_adicionales()
    {
        var lavados = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", "Lavados.razor"));
        var lavadosCs = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", "Lavados.razor.cs"));
        var ventas = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", "Ventas.razor"));
        var ventasCs = File.ReadAllText(Path.Combine(Raiz(), "src", "AutoBrillo.Web", "Pages", "Ventas.razor.cs"));
        Assert.Contains("multiple", lavados);
        Assert.Contains("adicionalesSeleccionados", lavadosCs);
        Assert.Contains("Terminado", lavados);
        Assert.DoesNotContain("prompt", lavadosCs);
        Assert.DoesNotContain("prompt", ventasCs);
        Assert.Contains("CobroDe", ventasCs);
        Assert.DoesNotContain("importeCobro", ventas);
        Assert.Contains("LavadoDisponibleParaVenta", ventas);
    }

    [Fact]
    public void La_migracion_de_precios_es_idempotente_y_el_ajuste_de_stock_no_esta_ahi()
    {
        var carpeta = Path.Combine(Raiz(), "supabase", "migrations");
        var precios = File.ReadAllText(Directory.GetFiles(carpeta, "*historial_precios_vigentes.sql").Single());
        var ventas = File.ReadAllText(Directory.GetFiles(carpeta, "*venta_activa_por_lavado_y_patentes.sql").Single());
        Assert.Contains("add column if not exists", precios);
        Assert.Contains("ux_precios_servicios_vigente", precios);
        Assert.Contains("drop constraint if exists", precios);
        Assert.DoesNotContain("drop table", precios.ToLowerInvariant());
        Assert.Contains("create unique index if not exists ux_ventas_lavado_activa", ventas);
        Assert.DoesNotContain("opcional_reconciliar_stock.sql", string.Join('\n', Directory.GetFiles(carpeta)));
        Assert.True(File.Exists(Path.Combine(Raiz(), "supabase", "scripts", "opcional_reconciliar_stock.sql")));
        Assert.Contains("dotnet test", File.ReadAllText(Path.Combine(Raiz(), ".github", "workflows", "build-test.yml")));
    }

    private static Usuario Usuario(int id, bool activo, string rol) => new()
    {
        Id_Usuarios = id,
        Nombre = "u" + id,
        Activo = activo,
        Roles = [new Rol { Id_Roles = id, Descripcion = rol }]
    };

    private static UsuariosController Controlador(IUserRepository usuarios, int idAutenticado, params Rol[] roles)
    {
        var controller = new UsuariosController(usuarios, new RolesFijos(roles), new PasswordHasher(), new PersonasVacias());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, idAutenticado.ToString())], "test"))
            }
        };
        return controller;
    }

    private static string Extraer(string codigo, string firma)
    {
        var inicio = codigo.IndexOf(firma, StringComparison.Ordinal);
        var siguiente = codigo.IndexOf("public async Task", inicio + firma.Length, StringComparison.Ordinal);
        if (siguiente < 0) siguiente = codigo.IndexOf("public Task", inicio + firma.Length, StringComparison.Ordinal);
        return codigo[inicio..siguiente];
    }

    private static string Raiz()
    {
        var carpeta = new DirectoryInfo(AppContext.BaseDirectory);
        while (carpeta is not null && !File.Exists(Path.Combine(carpeta.FullName, "AutoBrillo.slnx"))) carpeta = carpeta.Parent;
        return carpeta!.FullName;
    }

    private sealed class AmbitoSqlite : IDisposable
    {
        private readonly Microsoft.Data.Sqlite.SqliteConnection conexion = new("Data Source=:memory:");
        public AutoBrilloDbContext Db { get; }
        public AmbitoSqlite()
        {
            conexion.Open();
            Db = new AutoBrilloDbContext(new DbContextOptionsBuilder<AutoBrilloDbContext>().UseSqlite(conexion).Options);
        }
        public void Dispose() { Db.Dispose(); conexion.Dispose(); }
    }

    private sealed class ProductosFijos(Producto producto) : IProductoRepository
    {
        public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Producto?>(id == producto.Id_Productos ? producto : null);
        public Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Producto> { producto });
        public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Producto entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Producto> CrearConStockInicialAsync(Producto entidad, decimal stockInicial, int idUsuario, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Eliminar(Producto entidad) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TiposFijos(TipoProducto tipo) : ITipoProductoRepository
    {
        public Task<TipoProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<TipoProducto?>(id == tipo.Id_Tipos_Productos ? tipo : null);
        public Task<List<TipoProducto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<TipoProducto> { tipo });
        public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> TieneProductosAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(TipoProducto entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(TipoProducto entidad) { }
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class UsuariosEnMemoria(params Usuario[] usuarios) : IUserRepository
    {
        public List<Usuario> Items { get; } = [.. usuarios];
        public bool Elimino { get; private set; }
        public Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items);
        public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id_Usuarios == id));
        public Task<Usuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> PersonaYaVinculadaAsync(int idPersona, int? excluirUsuarioId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AgregarAsync(Usuario entidad, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Eliminar(Usuario entidad) => Elimino = true;
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RolesFijos(params Rol[] roles) : IRolRepository
    {
        public Task<Rol?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult(roles.FirstOrDefault(x => x.Id_Roles == id));
        public Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default) => Task.FromResult(roles.ToList());
        public Task<Rol?> ObtenerPorDescripcionAsync(string descripcion, CancellationToken cancellationToken = default) => Task.FromResult(roles.FirstOrDefault(x => x.Descripcion == descripcion));
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
}
