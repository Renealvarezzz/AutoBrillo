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

        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                propiedad.SetColumnType("timestamp without time zone");
    }
}
