using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

/// <summary>
/// Implementación concreta del contrato IUserRepository.
/// Centraliza las consultas de usuarios para que la API no escriba SQL ni conozca Entity Framework Core.
/// </summary>
public class RepositorioUsuarios(AutoBrilloDbContext contexto) : IUserRepository
{
    /// <summary>Obtiene todos los usuarios y sus roles para la grilla del ABM.</summary>
    public Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        contexto.Usuarios.Include(x => x.Roles).OrderBy(x => x.Nombre).ToListAsync(cancellationToken);

    /// <summary>Busca un usuario por su clave primaria junto con sus roles.</summary>
    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.Usuarios.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id_Usuarios == id, cancellationToken);

    /// <summary>
    /// Consulta un único usuario cuyo Nombre coincida exactamente con el valor indicado.
    /// SingleOrDefaultAsync devuelve null cuando la consulta no encuentra ninguno.
    /// </summary>
    public Task<Usuario?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default) =>
        contexto.Usuarios.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Nombre == nombre, cancellationToken);

    /// <summary>
    /// Pregunta a la base si existe al menos un usuario con ese nombre.
    /// Se usa antes de registrar para evitar nombres duplicados.
    /// </summary>
    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default) =>
        contexto.Usuarios.AnyAsync(x => x.Nombre == nombre, cancellationToken);

    /// <summary>
    /// Agrega el objeto al contexto de EF Core, pero todavía no ejecuta el INSERT.
    /// El INSERT ocurre después en GuardarCambiosAsync.
    /// </summary>
    public Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        contexto.Usuarios.AddAsync(usuario, cancellationToken).AsTask();

    /// <summary>Marca el usuario para eliminarlo en el próximo GuardarCambiosAsync.</summary>
    public void Eliminar(Usuario usuario) => contexto.Usuarios.Remove(usuario);

    /// <summary>Envía a PostgreSQL los cambios pendientes, por ejemplo el INSERT de un usuario nuevo.</summary>
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        contexto.SaveChangesAsync(cancellationToken);
}
