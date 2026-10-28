using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public class RepositorioTiposPersonas(AutoBrilloDbContext contexto) : ITipoPersonaRepository
{
    public Task<List<TipoPersona>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        contexto.TiposPersonas.OrderBy(x => x.Descripcion).ToListAsync(cancellationToken);

    public Task<TipoPersona?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.TiposPersonas.SingleOrDefaultAsync(x => x.Id_Tipos_Personas == id, cancellationToken);

    public Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default) =>
        contexto.TiposPersonas.AnyAsync(x => x.Descripcion == descripcion && (!excluirId.HasValue || x.Id_Tipos_Personas != excluirId), cancellationToken);

    public Task<bool> TienePersonasAsync(int id, CancellationToken cancellationToken = default) =>
        contexto.Personas.AnyAsync(x => x.Tipos.Any(tipo => tipo.Id_Tipos_Personas == id), cancellationToken);

    public Task AgregarAsync(TipoPersona tipoPersona, CancellationToken cancellationToken = default) => contexto.TiposPersonas.AddAsync(tipoPersona, cancellationToken).AsTask();
    public void Eliminar(TipoPersona tipoPersona) => contexto.TiposPersonas.Remove(tipoPersona);
    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => contexto.SaveChangesAsync(cancellationToken);
}
