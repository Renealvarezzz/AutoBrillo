using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using AutoBrillo.Repository.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AutoBrillo.Web.Tests;

public class Sprint3RepositorioTests
{
    [Fact]
    public async Task La_fecha_utc_se_normaliza_antes_de_ocupar_el_sector()
    {
        await using var ambito = await Ambito.CrearAsync();
        var utc = new DateTime(2026, 3, 10, 15, 0, 0, DateTimeKind.Utc);
        var lavado = await ambito.Sprint.RegistrarLavadoAsync(ambito.NuevoLavado(utc, 1), ambito.UsuarioId, CancellationToken.None);

        Assert.Equal(DateTimeKind.Unspecified, lavado.Fecha.Kind);
        Assert.Equal(ServicioOperativo.NormalizarFecha(utc), lavado.Fecha);

        var error = await Assert.ThrowsAsync<ReglaNegocioException>(() => ambito.Sprint.RegistrarLavadoAsync(ambito.NuevoLavado(utc, 1), ambito.UsuarioId, CancellationToken.None));
        Assert.Contains("ocupado", error.Message);
    }

    [Fact]
    public async Task Una_venta_de_lavado_terminado_comisiona_solo_servicios_y_no_se_duplica()
    {
        await using var ambito = await Ambito.CrearAsync();
        var lavado = await ambito.Sprint.RegistrarLavadoAsync(ambito.NuevoLavado(new DateTime(2026, 3, 11, 9, 0, 0), 2), ambito.UsuarioId, CancellationToken.None);
        await ambito.Sprint.FinalizarLavadoAsync(lavado.Id_Lavados, ambito.UsuarioId, CancellationToken.None);
        Assert.Equal(8m, (await ambito.Db.Productos.SingleAsync(x => x.Id_Productos == ambito.InsumoId)).StockActual);

        var venta = await ambito.Sprint.RegistrarVentaAsync(ambito.NuevaVenta(lavado.Id_Lavados), ambito.UsuarioId, CancellationToken.None);
        var comision = await ambito.Db.Comisiones.SingleAsync();

        Assert.Equal(63000m, venta.Total);
        Assert.Equal(16500m, comision.Importe);
        Assert.Equal(19m, (await ambito.Db.Productos.SingleAsync(x => x.Id_Productos == ambito.ShampooId)).StockActual);

        var duplicada = await Assert.ThrowsAsync<ReglaNegocioException>(() => ambito.Sprint.RegistrarVentaAsync(ambito.NuevaVenta(lavado.Id_Lavados), ambito.UsuarioId, CancellationToken.None));
        Assert.Contains("venta activa", duplicada.Message);

        await ambito.Sprint.AnularVentaAsync(venta.Id_Ventas, "carga duplicada", ambito.UsuarioId, CancellationToken.None);
        var otra = await ambito.Sprint.RegistrarVentaAsync(ambito.NuevaVenta(lavado.Id_Lavados), ambito.UsuarioId, CancellationToken.None);
        Assert.True(otra.Id_Ventas > 0);
        Assert.Equal(19m, (await ambito.Db.Productos.SingleAsync(x => x.Id_Productos == ambito.ShampooId)).StockActual);
    }

    [Fact]
    public async Task Cancelar_un_lavado_terminado_devuelve_insumos_y_anula_la_venta()
    {
        await using var ambito = await Ambito.CrearAsync();
        var lavado = await ambito.Sprint.RegistrarLavadoAsync(ambito.NuevoLavado(new DateTime(2026, 3, 12, 9, 0, 0), 3), ambito.UsuarioId, CancellationToken.None);
        await ambito.Sprint.FinalizarLavadoAsync(lavado.Id_Lavados, ambito.UsuarioId, CancellationToken.None);
        var venta = await ambito.Sprint.RegistrarVentaAsync(ambito.NuevaVenta(lavado.Id_Lavados), ambito.UsuarioId, CancellationToken.None);

        await ambito.Sprint.AnularLavadoAsync(lavado.Id_Lavados, "el cliente se retiró", ambito.UsuarioId, CancellationToken.None);

        Assert.Equal("Cancelado", (await ambito.Db.Lavados.SingleAsync(x => x.Id_Lavados == lavado.Id_Lavados)).Estado);
        Assert.Equal("Anulada", (await ambito.Db.Ventas.SingleAsync(x => x.Id_Ventas == venta.Id_Ventas)).Estado);
        Assert.True((await ambito.Db.Comisiones.SingleAsync()).Anulada);
        Assert.Equal(10m, (await ambito.Db.Productos.SingleAsync(x => x.Id_Productos == ambito.InsumoId)).StockActual);
        Assert.Equal(20m, (await ambito.Db.Productos.SingleAsync(x => x.Id_Productos == ambito.ShampooId)).StockActual);
    }

    [Fact]
    public async Task Una_venta_anulada_no_bloquea_la_edicion_del_lavado()
    {
        await using var ambito = await Ambito.CrearAsync();
        var lavado = await ambito.Sprint.RegistrarLavadoAsync(ambito.NuevoLavado(new DateTime(2026, 3, 13, 9, 0, 0), 4), ambito.UsuarioId, CancellationToken.None);
        var venta = await ambito.Sprint.RegistrarVentaAsync(ambito.NuevaVenta(lavado.Id_Lavados), ambito.UsuarioId, CancellationToken.None);
        var bloqueado = await Assert.ThrowsAsync<ReglaNegocioException>(() => ambito.Sprint.ActualizarLavadoAsync(lavado.Id_Lavados, ambito.NuevoLavado(lavado.Fecha, 5), CancellationToken.None));
        Assert.Contains("venta activa", bloqueado.Message);

        await ambito.Sprint.AnularVentaAsync(venta.Id_Ventas, "se anuló el cobro", ambito.UsuarioId, CancellationToken.None);
        var editado = await ambito.Sprint.ActualizarLavadoAsync(lavado.Id_Lavados, ambito.NuevoLavado(lavado.Fecha, 5), CancellationToken.None);
        Assert.Equal(5, editado.Sector);
    }

    [Fact]
    public async Task Guardar_un_precio_conserva_el_anterior_y_la_consulta_usa_el_vigente()
    {
        await using var ambito = await Ambito.CrearAsync();
        var primero = await ambito.Catalogos.GuardarPrecioAsync(ambito.ServicioId, ambito.TipoVehiculoId, 45000, ambito.UsuarioId, CancellationToken.None);
        var segundo = await ambito.Catalogos.GuardarPrecioAsync(ambito.ServicioId, ambito.TipoVehiculoId, 52000, ambito.UsuarioId, CancellationToken.None);

        Assert.False((await ambito.Db.PreciosServicios.SingleAsync(x => x.Id_Precios_Servicios == primero.Id_Precios_Servicios)).Vigente);
        Assert.True(segundo.Vigente);
        Assert.Equal(52000m, await ambito.Operativo.ResolverPrecioAsync(ambito.ServicioId, ambito.TipoVehiculoId, CancellationToken.None));
        Assert.Equal(52000m, (await ambito.Catalogos.ObtenerPreciosAsync()).Single(x => x.Id_Productos == ambito.ServicioId).Precio);
    }

    [Fact]
    public async Task El_stock_inicial_queda_como_movimiento_y_la_patente_ignora_espacios()
    {
        await using var ambito = await Ambito.CrearAsync();
        var creado = await ambito.Productos.CrearConStockInicialAsync(new Producto
        {
            Descripcion = "Cera líquida",
            PrecioDeCosto = 1000,
            PrecioDeVenta = 2000,
            Tipos = [await ambito.Db.TiposProductos.SingleAsync(x => x.Descripcion == "Producto")]
        }, 7, ambito.UsuarioId, CancellationToken.None);

        Assert.Equal(7m, creado.StockActual);
        var movimiento = await ambito.Db.MovimientosStock.SingleAsync(x => x.Id_Productos == creado.Id_Productos);
        Assert.Equal(ReglasNegocio.Entrada, movimiento.Tipo);
        Assert.Equal(7m, movimiento.Cantidad);
        Assert.Equal("Ajuste", movimiento.Origen);

        await ambito.Vehiculos.GuardarAsync(new Vehiculo { Patente = "abc 123", Id_Personas = ambito.ClienteId, Id_Tipos_Vehiculos = ambito.TipoVehiculoId, Activo = true }, CancellationToken.None);
        var repetida = await Assert.ThrowsAsync<ReglaNegocioException>(() => ambito.Vehiculos.GuardarAsync(new Vehiculo { Patente = " ABC123 ", Id_Personas = ambito.ClienteId, Id_Tipos_Vehiculos = ambito.TipoVehiculoId, Activo = true }, CancellationToken.None));
        Assert.Contains("patente", repetida.Message);
    }

    private sealed class Ambito : IAsyncDisposable
    {
        private readonly SqliteConnection conexion;
        public AutoBrilloDbContext Db { get; }
        public ServicioOperativo Operativo { get; }
        public RepositorioSprintTres Sprint { get; }
        public RepositorioCatalogos Catalogos { get; }
        public RepositorioProductos Productos { get; }
        public RepositorioVehiculos Vehiculos { get; }
        public int UsuarioId { get; private set; }
        public int ClienteId { get; private set; }
        public int FuncionarioId { get; private set; }
        public int ServicioId { get; private set; }
        public int AdicionalId { get; private set; }
        public int ShampooId { get; private set; }
        public int InsumoId { get; private set; }
        public int TipoVehiculoId { get; private set; }
        public int VehiculoId { get; private set; }

        private Ambito(SqliteConnection conexion, AutoBrilloDbContext db)
        {
            this.conexion = conexion;
            Db = db;
            Operativo = new ServicioOperativo(db);
            Sprint = new RepositorioSprintTres(db, Operativo);
            Catalogos = new RepositorioCatalogos(db, Operativo);
            Productos = new RepositorioProductos(db, Operativo);
            Vehiculos = new RepositorioVehiculos(db, Operativo);
        }

        public static async Task<Ambito> CrearAsync()
        {
            var conexion = new SqliteConnection("Data Source=:memory:");
            await conexion.OpenAsync();
            var db = new AutoBrilloDbContext(new DbContextOptionsBuilder<AutoBrilloDbContext>().UseSqlite(conexion).Options);
            db.Database.EnsureCreated();
            var ambito = new Ambito(conexion, db);
            await ambito.SembrarAsync();
            return ambito;
        }

        public Lavado NuevoLavado(DateTime fecha, int sector) => new()
        {
            Id_Vehiculos = VehiculoId,
            Sector = sector,
            Fecha = fecha,
            Servicios =
            [
                new LavadoServicio { Id_Productos = ServicioId },
                new LavadoServicio { Id_Productos = AdicionalId, EsAdicional = true }
            ],
            Participantes = [new LavadoParticipante { Id_Personas = FuncionarioId }]
        };

        public Venta NuevaVenta(int idLavado) => new()
        {
            Id_Lavados = idLavado,
            Condicion = "Contado",
            Detalles =
            [
                new VentaDetalle { Id_Productos = ServicioId, Cantidad = 1 },
                new VentaDetalle { Id_Productos = AdicionalId, Cantidad = 1 },
                new VentaDetalle { Id_Productos = ShampooId, Cantidad = 1 }
            ]
        };

        private async Task SembrarAsync()
        {
            var usuario = new Usuario { Nombre = "admin", Password = "x", Activo = true };
            var clienteTipo = new TipoPersona { Descripcion = "Cliente" };
            var funcionarioTipo = new TipoPersona { Descripcion = "Funcionario" };
            var cliente = new Persona { Nombre = "Ana", Documento = "1", Activo = true, Tipos = [clienteTipo] };
            var funcionario = new Persona { Nombre = "Luis", Documento = "2", Activo = true, Tipos = [funcionarioTipo] };
            var tipoServicio = new TipoProducto { Descripcion = "Servicio" };
            var tipoAdicional = new TipoProducto { Descripcion = "Adicional" };
            var tipoProducto = new TipoProducto { Descripcion = "Producto" };
            var tipoInsumo = new TipoProducto { Descripcion = "Insumo" };
            var servicio = new Producto { Descripcion = "Lavado completo", PrecioDeVenta = 40000, Activo = true, Tipos = [tipoServicio] };
            var adicional = new Producto { Descripcion = "Cera", PrecioDeVenta = 10000, Activo = true, Tipos = [tipoAdicional] };
            var shampoo = new Producto { Descripcion = "Shampoo", PrecioDeVenta = 8000, PrecioDeCosto = 3000, StockActual = 20, Activo = true, Tipos = [tipoProducto] };
            var insumo = new Producto { Descripcion = "Agua", StockActual = 10, Activo = true, Tipos = [tipoInsumo] };
            var tipoAuto = new TipoVehiculo { Descripcion = "Auto", Activo = true };
            Db.AddRange(usuario, cliente, funcionario, servicio, adicional, shampoo, insumo, tipoAuto);
            await Db.SaveChangesAsync();

            var vehiculo = new Vehiculo { Patente = "XYZ999", Id_Personas = cliente.Id_Personas, Id_Tipos_Vehiculos = tipoAuto.Id_Tipos_Vehiculos, Activo = true };
            Db.Vehiculos.Add(vehiculo);
            Db.PreciosServicios.Add(new PrecioServicio
            {
                Id_Productos = servicio.Id_Productos,
                Id_Tipos_Vehiculos = tipoAuto.Id_Tipos_Vehiculos,
                Precio = 45000,
                Vigente = true,
                FechaActualizacion = ServicioOperativo.Ahora(),
                Id_Usuarios = usuario.Id_Usuarios
            });
            Db.ServiciosInsumos.Add(new ServicioInsumo { Id_Servicios = servicio.Id_Productos, Id_Insumos = insumo.Id_Productos, Cantidad = 2 });
            await Db.SaveChangesAsync();

            UsuarioId = usuario.Id_Usuarios;
            ClienteId = cliente.Id_Personas;
            FuncionarioId = funcionario.Id_Personas;
            ServicioId = servicio.Id_Productos;
            AdicionalId = adicional.Id_Productos;
            ShampooId = shampoo.Id_Productos;
            InsumoId = insumo.Id_Productos;
            TipoVehiculoId = tipoAuto.Id_Tipos_Vehiculos;
            VehiculoId = vehiculo.Id_Vehiculos;
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await conexion.DisposeAsync();
        }
    }
}
