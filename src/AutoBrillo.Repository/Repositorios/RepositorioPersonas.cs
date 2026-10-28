using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public class RepositorioPersonas(AutoBrilloDbContext contexto) : IPersonaRepository
{
    public Task<List<Persona>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        contexto.Personas.Include(x => x.Tipos).OrderBy(x => x.Nombre).ToListAsync(cancellationToken);

    public Task<Persona?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.Personas.Include(x => x.Tipos).SingleOrDefaultAsync(x => x.Id_Personas == id, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken cancellationToken = default) =>
        contexto.Personas.AnyAsync(x => x.Documento == documento && (!excluirId.HasValue || x.Id_Personas != excluirId), cancellationToken);

    public Task AgregarAsync(Persona persona, CancellationToken cancellationToken = default) => contexto.Personas.AddAsync(persona, cancellationToken).AsTask();
    public void Eliminar(Persona persona) => contexto.Personas.Remove(persona);
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => contexto.SaveChangesAsync(cancellationToken);
}
