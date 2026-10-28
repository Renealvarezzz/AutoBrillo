using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

/// <summary>Implementación EF Core del ABM de roles.</summary>
public class RepositorioRoles(AutoBrilloDbContext contexto) : IRolRepository
{
    public Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        contexto.Roles.OrderBy(x => x.Descripcion).ToListAsync(cancellationToken);

    public Task<Rol?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.Roles.SingleOrDefaultAsync(x => x.Id_Roles == id, cancellationToken);

    public Task<Rol?> ObtenerPorDescripcionAsync(string descripcion, CancellationToken cancellationToken = default) =>
        contexto.Roles.SingleOrDefaultAsync(x => x.Descripcion == descripcion, cancellationToken);

    public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) =>
        contexto.Roles.AnyAsync(x => x.Descripcion == descripcion && (!excluirId.HasValue || x.Id_Roles != excluirId), cancellationToken);

    public Task<bool> TieneUsuariosAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.Usuarios.AnyAsync(x => x.Roles.Any(rol => rol.Id_Roles == id), cancellationToken);

    public Task AgregarAsync(Rol rol, CancellationToken cancellationToken = default) =>
        contexto.Roles.AddAsync(rol, cancellationToken).AsTask();

    public void Eliminar(Rol rol) => contexto.Roles.Remove(rol);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        contexto.SaveChangesAsync(cancellationToken);
}
