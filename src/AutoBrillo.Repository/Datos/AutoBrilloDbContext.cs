using AutoBrillo.Model.Entidades;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Datos;

/// <summary>
/// Puente entre las clases de C# y las tablas de PostgreSQL.
/// EF Core usa esta clase para saber qué tabla y qué columnas debe consultar.
/// No depende de Supabase; por eso facilita una futura migración a otra base compatible.
/// </summary>
public class AutoBrilloDbContext(DbContextOptions<AutoBrilloDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Representa la tabla usuarios dentro del código.
    /// Ejemplo: contexto.Usuarios permite hacer consultas con LINQ.
    /// </summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Representa la tabla roles dentro del código.</summary>
    public DbSet<Rol> Roles => Set<Rol>();

    /// <summary>Representa la tabla personas dentro del código.</summary>
    public DbSet<Persona> Personas => Set<Persona>();

    /// <summary>Representa el catálogo de tipos de personas.</summary>
    public DbSet<TipoPersona> TiposPersonas => Set<TipoPersona>();

    /// <summary>Representa el registro de productos.</summary>
    public DbSet<Producto> Productos => Set<Producto>();

    /// <summary>Representa el catálogo de tipos de productos.</summary>
    public DbSet<TipoProducto> TiposProductos => Set<TipoProducto>();

    /// <summary>
    /// Configura cómo se relaciona Usuario con la tabla real de la base de datos.
    /// Se ejecuta cuando EF Core construye su modelo interno.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var usuario = modelBuilder.Entity<Usuario>();

        // La tabla real se llama exactamente usuarios y está en minúsculas.
        usuario.ToTable("usuarios");

        // Id_Usuarios es la clave primaria de cada registro.
        usuario.HasKey(x => x.Id_Usuarios);
        usuario.Property(x => x.Id_Usuarios).HasColumnName("Id_Usuarios");

        // Nombre es obligatorio, admite hasta 100 caracteres y debe ser único.
        usuario.Property(x => x.Nombre).HasColumnName("Nombre").HasMaxLength(100).IsRequired();
        usuario.HasIndex(x => x.Nombre).IsUnique();

        // Password es obligatorio y guarda el hash BCrypt, no el texto original.
        usuario.Property(x => x.Password).HasColumnName("Password").IsRequired();

        // Activo controla el acceso: los nuevos usuarios se crean habilitados.
        usuario.Property(x => x.Activo).HasColumnName("Activo").HasDefaultValue(true).IsRequired();

        // Un usuario puede tener varios roles y cada rol puede pertenecer a varios usuarios.
        // La tabla intermedia usuarios_roles guarda cada asignación sin duplicados.
        usuario.HasMany(x => x.Roles)
            .WithMany(x => x.Usuarios)
            .UsingEntity<Dictionary<string, object>>(
                "usuarios_roles",
                relacion => relacion.HasOne<Rol>().WithMany().HasForeignKey("Id_Roles").OnDelete(DeleteBehavior.Restrict),
                relacion => relacion.HasOne<Usuario>().WithMany().HasForeignKey("Id_Usuarios").OnDelete(DeleteBehavior.Cascade),
                relacion =>
                {
                    relacion.ToTable("usuarios_roles");
                    relacion.HasKey("Id_Usuarios", "Id_Roles");
                    relacion.IndexerProperty<int>("Id_Usuarios").HasColumnName("Id_Usuarios");
                    relacion.IndexerProperty<int>("Id_Roles").HasColumnName("Id_Roles");
                });

        var rol = modelBuilder.Entity<Rol>();
        rol.ToTable("roles");
        rol.HasKey(x => x.Id_Roles);
        rol.Property(x => x.Id_Roles).HasColumnName("Id_Roles");
        rol.Property(x => x.Descripcion).HasColumnName("Descripcion").HasMaxLength(100).IsRequired();
        rol.HasIndex(x => x.Descripcion).IsUnique();

        var persona = modelBuilder.Entity<Persona>();
        persona.ToTable("personas");
        persona.HasKey(x => x.Id_Personas);
        persona.Property(x => x.Id_Personas).HasColumnName("Id_Personas");
        persona.Property(x => x.Nombre).HasColumnName("Nombre").HasMaxLength(150).IsRequired();
        persona.Property(x => x.Documento).HasColumnName("Documento").HasMaxLength(50).IsRequired();
        persona.Property(x => x.Telefono).HasColumnName("Telefono").HasMaxLength(30);
        persona.Property(x => x.Activo).HasColumnName("Activo").HasDefaultValue(true).IsRequired();
        persona.HasIndex(x => x.Documento).IsUnique();
        persona.HasMany(x => x.Tipos)
            .WithMany(x => x.Personas)
            .UsingEntity<Dictionary<string, object>>(
                "personas_tipos",
                relacion => relacion.HasOne<TipoPersona>().WithMany().HasForeignKey("Id_Tipos_Personas").OnDelete(DeleteBehavior.Restrict),
                relacion => relacion.HasOne<Persona>().WithMany().HasForeignKey("Id_Personas").OnDelete(DeleteBehavior.Cascade),
                relacion =>
                {
                    relacion.ToTable("personas_tipos");
                    relacion.HasKey("Id_Personas", "Id_Tipos_Personas");
                    relacion.IndexerProperty<int>("Id_Personas").HasColumnName("Id_Personas");
                    relacion.IndexerProperty<int>("Id_Tipos_Personas").HasColumnName("Id_Tipos_Personas");
                });

        var tipoPersona = modelBuilder.Entity<TipoPersona>();
        tipoPersona.ToTable("tipos_personas");
        tipoPersona.HasKey(x => x.Id_Tipos_Personas);
        tipoPersona.Property(x => x.Id_Tipos_Personas).HasColumnName("Id_Tipos_Personas");
        tipoPersona.Property(x => x.Descripcion).HasColumnName("Descripcion").HasMaxLength(100).IsRequired();
        tipoPersona.HasIndex(x => x.Descripcion).IsUnique();

        var producto = modelBuilder.Entity<Producto>();
        producto.ToTable("productos");
        producto.HasKey(x => x.Id_Productos);
        producto.Property(x => x.Id_Productos).HasColumnName("Id_Productos");
        producto.Property(x => x.Descripcion).HasColumnName("Descripcion").HasMaxLength(150).IsRequired();
        producto.Property(x => x.PrecioDeCosto).HasColumnName("PrecioDeCosto").HasPrecision(18, 2).IsRequired();
        producto.Property(x => x.PrecioDeVenta).HasColumnName("PrecioDeVenta").HasPrecision(18, 2).IsRequired();
        producto.Property(x => x.Activo).HasColumnName("Activo").HasDefaultValue(true).IsRequired();
        producto.Property(x => x.StockActual).HasColumnName("StockActual").HasPrecision(18, 2).IsRequired();
        producto.HasIndex(x => x.Descripcion).IsUnique();
        producto.HasMany(x => x.Tipos)
            .WithMany(x => x.Productos)
            .UsingEntity<Dictionary<string, object>>(
                "productos_tipos",
                relacion => relacion.HasOne<TipoProducto>().WithMany().HasForeignKey("Id_Tipos_Productos").OnDelete(DeleteBehavior.Restrict),
                relacion => relacion.HasOne<Producto>().WithMany().HasForeignKey("Id_Productos").OnDelete(DeleteBehavior.Cascade),
                relacion =>
                {
                    relacion.ToTable("productos_tipos");
                    relacion.HasKey("Id_Productos", "Id_Tipos_Productos");
                    relacion.IndexerProperty<int>("Id_Productos").HasColumnName("Id_Productos");
                    relacion.IndexerProperty<int>("Id_Tipos_Productos").HasColumnName("Id_Tipos_Productos");
                });

        var tipoProducto = modelBuilder.Entity<TipoProducto>();
        tipoProducto.ToTable("tipos_productos");
        tipoProducto.HasKey(x => x.Id_Tipos_Productos);
        tipoProducto.Property(x => x.Id_Tipos_Productos).HasColumnName("Id_Tipos_Productos");
        tipoProducto.Property(x => x.Descripcion).HasColumnName("Descripcion").HasMaxLength(100).IsRequired();
        tipoProducto.HasIndex(x => x.Descripcion).IsUnique();
    }
}
