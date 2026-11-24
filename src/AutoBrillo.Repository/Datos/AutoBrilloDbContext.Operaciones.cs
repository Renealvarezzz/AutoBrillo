using AutoBrillo.Model.Entidades;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Datos;

/// <summary>Tablas del sprint 2. El esquema lo crean los SQL de supabase/migrations, no EF.</summary>
public partial class AutoBrilloDbContext
{
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<TipoVehiculo> TiposVehiculos => Set<TipoVehiculo>();
    public DbSet<PrecioServicio> PreciosServicios => Set<PrecioServicio>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<ServicioInsumo> ServiciosInsumos => Set<ServicioInsumo>();
    public DbSet<Lavado> Lavados => Set<Lavado>();
    public DbSet<LavadoServicio> LavadosServicios => Set<LavadoServicio>();
    public DbSet<LavadoParticipante> LavadosParticipantes => Set<LavadoParticipante>();
    public DbSet<LavadoHistorialEstado> LavadosHistorialEstados => Set<LavadoHistorialEstado>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentasDetalle => Set<VentaDetalle>();
    public DbSet<Cobro> Cobros => Set<Cobro>();
    public DbSet<ConfiguracionComision> ConfiguracionesComision => Set<ConfiguracionComision>();
    public DbSet<Comision> Comisiones => Set<Comision>();

    private static void ConfigurarOperaciones(ModelBuilder modelBuilder)
    {
        var usuario = modelBuilder.Entity<Usuario>();
        usuario.Property(x => x.Id_Personas).HasColumnName("Id_Personas");
        usuario.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.Id_Personas).OnDelete(DeleteBehavior.SetNull);
        usuario.HasIndex(x => x.Id_Personas).IsUnique().HasFilter("\"Id_Personas\" IS NOT NULL");

        var movimiento = modelBuilder.Entity<MovimientoStock>();
        movimiento.ToTable("movimientos_stock").HasKey(x => x.Id_Movimientos_Stock);
        movimiento.Property(x => x.Cantidad).HasPrecision(18, 0);
        movimiento.Property(x => x.StockResultante).HasPrecision(18, 0);
        movimiento.Property(x => x.Tipo).HasMaxLength(10);
        movimiento.Property(x => x.Origen).HasMaxLength(30);
        movimiento.Property(x => x.Observacion).HasMaxLength(250);
        movimiento.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.Id_Productos).OnDelete(DeleteBehavior.Restrict);

        var compra = modelBuilder.Entity<Compra>();
        compra.ToTable("compras").HasKey(x => x.Id_Compras);
        compra.Property(x => x.Comprobante).HasMaxLength(50);
        compra.Property(x => x.Total).HasPrecision(18, 0);
        compra.Property(x => x.Estado).HasMaxLength(20);
        compra.Property(x => x.MotivoAnulacion).HasMaxLength(250);
        compra.HasOne(x => x.Proveedor).WithMany().HasForeignKey(x => x.Id_Personas).OnDelete(DeleteBehavior.Restrict);
        compra.HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.Id_Compras).OnDelete(DeleteBehavior.Cascade);

        var detalle = modelBuilder.Entity<CompraDetalle>();
        detalle.ToTable("compras_detalle").HasKey(x => x.Id_Compras_Detalle);
        detalle.Property(x => x.Cantidad).HasPrecision(18, 0);
        detalle.Property(x => x.PrecioUnitario).HasPrecision(18, 0);
        detalle.Property(x => x.Subtotal).HasPrecision(18, 0);
        detalle.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.Id_Productos).OnDelete(DeleteBehavior.Restrict);

        var tipo = modelBuilder.Entity<TipoVehiculo>();
        tipo.ToTable("tipos_vehiculos").HasKey(x => x.Id_Tipos_Vehiculos);
        tipo.Property(x => x.Descripcion).HasMaxLength(100).IsRequired();
        tipo.HasIndex(x => x.Descripcion).IsUnique();

        var precio = modelBuilder.Entity<PrecioServicio>();
        precio.ToTable("precios_servicios").HasKey(x => x.Id_Precios_Servicios);
        precio.Property(x => x.Precio).HasPrecision(18, 0);
        precio.HasIndex(x => new { x.Id_Productos, x.Id_Tipos_Vehiculos }).IsUnique();
        precio.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.Id_Productos).OnDelete(DeleteBehavior.Cascade);
        precio.HasOne(x => x.TipoVehiculo).WithMany().HasForeignKey(x => x.Id_Tipos_Vehiculos).OnDelete(DeleteBehavior.Restrict);

        var vehiculo = modelBuilder.Entity<Vehiculo>();
        vehiculo.ToTable("vehiculos").HasKey(x => x.Id_Vehiculos);
        vehiculo.Property(x => x.Patente).HasMaxLength(20).IsRequired();
        vehiculo.Property(x => x.Marca).HasMaxLength(80);
        vehiculo.Property(x => x.Modelo).HasMaxLength(80);
        vehiculo.Property(x => x.Color).HasMaxLength(40);
        vehiculo.HasIndex(x => x.Patente).IsUnique();
        vehiculo.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.Id_Personas).OnDelete(DeleteBehavior.Restrict);
        vehiculo.HasOne(x => x.TipoVehiculo).WithMany().HasForeignKey(x => x.Id_Tipos_Vehiculos).OnDelete(DeleteBehavior.Restrict);

        var insumo = modelBuilder.Entity<ServicioInsumo>();
        insumo.ToTable("servicios_insumos").HasKey(x => x.Id_Servicios_Insumos);
        insumo.Property(x => x.Cantidad).HasPrecision(18, 0);
        insumo.HasIndex(x => new { x.Id_Servicios, x.Id_Insumos }).IsUnique();
        insumo.HasOne(x => x.Servicio).WithMany().HasForeignKey(x => x.Id_Servicios).OnDelete(DeleteBehavior.Cascade);
        insumo.HasOne(x => x.Insumo).WithMany().HasForeignKey(x => x.Id_Insumos).OnDelete(DeleteBehavior.Restrict);

        var lavado = modelBuilder.Entity<Lavado>();
        lavado.ToTable("lavados").HasKey(x => x.Id_Lavados);
        lavado.Property(x => x.Estado).HasMaxLength(20);
        lavado.Property(x => x.MotivoAnulacion).HasMaxLength(250);
        lavado.HasOne(x => x.Vehiculo).WithMany().HasForeignKey(x => x.Id_Vehiculos).OnDelete(DeleteBehavior.Restrict);
        lavado.HasMany(x => x.Servicios).WithOne().HasForeignKey(x => x.Id_Lavados).OnDelete(DeleteBehavior.Cascade);
        lavado.HasMany(x => x.Participantes).WithOne().HasForeignKey(x => x.Id_Lavados).OnDelete(DeleteBehavior.Cascade);
        lavado.HasMany(x => x.HistorialEstados).WithOne().HasForeignKey(x => x.Id_Lavados).OnDelete(DeleteBehavior.Cascade);
        lavado.HasIndex(x => new { x.Sector, x.Fecha }).HasFilter("\"Estado\" = 'Registrado'").IsUnique();

        var lavadoServicio = modelBuilder.Entity<LavadoServicio>();
        lavadoServicio.ToTable("lavados_servicios").HasKey(x => x.Id_Lavados_Servicios);
        lavadoServicio.Property(x => x.PrecioAplicado).HasPrecision(18, 0);
        lavadoServicio.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.Id_Productos).OnDelete(DeleteBehavior.Restrict);
        var participante = modelBuilder.Entity<LavadoParticipante>();
        participante.ToTable("lavados_participantes").HasKey(x => new { x.Id_Lavados, x.Id_Personas });
        participante.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.Id_Personas).OnDelete(DeleteBehavior.Restrict);
        var estadoLavado = modelBuilder.Entity<LavadoHistorialEstado>();
        estadoLavado.ToTable("lavados_historial_estados").HasKey(x => x.Id_Lavados_Historial);
        estadoLavado.Property(x => x.Estado).HasMaxLength(20);
        estadoLavado.HasOne<Usuario>().WithMany().HasForeignKey(x => x.Id_Usuarios).OnDelete(DeleteBehavior.Restrict);

        var venta = modelBuilder.Entity<Venta>();
        venta.ToTable("ventas").HasKey(x => x.Id_Ventas);
        venta.Property(x => x.Condicion).HasMaxLength(10);
        venta.Property(x => x.Total).HasPrecision(18, 0);
        venta.Property(x => x.Estado).HasMaxLength(20);
        venta.Property(x => x.MotivoAnulacion).HasMaxLength(250);
        venta.HasOne(x => x.Lavado).WithMany().HasForeignKey(x => x.Id_Lavados).OnDelete(DeleteBehavior.Restrict);
        venta.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.Id_Usuarios).OnDelete(DeleteBehavior.Restrict);
        venta.HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.Id_Ventas).OnDelete(DeleteBehavior.Cascade);
        venta.HasMany(x => x.Cobros).WithOne().HasForeignKey(x => x.Id_Ventas).OnDelete(DeleteBehavior.Cascade);
        venta.HasMany(x => x.Comisiones).WithOne(x => x.Venta).HasForeignKey(x => x.Id_Ventas).OnDelete(DeleteBehavior.Cascade);
        var ventaDetalle = modelBuilder.Entity<VentaDetalle>();
        ventaDetalle.ToTable("ventas_detalle").HasKey(x => x.Id_Ventas_Detalle);
        ventaDetalle.Property(x => x.Cantidad).HasPrecision(18, 0);
        ventaDetalle.Property(x => x.PrecioUnitario).HasPrecision(18, 0);
        ventaDetalle.Property(x => x.Subtotal).HasPrecision(18, 0);
        ventaDetalle.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.Id_Productos).OnDelete(DeleteBehavior.Restrict);
        var cobro = modelBuilder.Entity<Cobro>();
        cobro.ToTable("cobros").HasKey(x => x.Id_Cobros);
        cobro.Property(x => x.MedioPago).HasMaxLength(20);
        cobro.Property(x => x.Importe).HasPrecision(18, 0);
        var config = modelBuilder.Entity<ConfiguracionComision>();
        config.ToTable("configuracion_comision").HasKey(x => x.Id_Configuracion_Comision);
        config.Property(x => x.Porcentaje).HasPrecision(5, 2);
        var comision = modelBuilder.Entity<Comision>();
        comision.ToTable("comisiones").HasKey(x => x.Id_Comisiones);
        comision.Property(x => x.PorcentajeAplicado).HasPrecision(5, 2);
        comision.Property(x => x.Importe).HasPrecision(18, 2);
        comision.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.Id_Personas).OnDelete(DeleteBehavior.Restrict);

        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                propiedad.SetColumnType("timestamp without time zone");
    }
}
