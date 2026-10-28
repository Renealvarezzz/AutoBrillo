using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Repository.Repositorios;

public interface ITipoPersonaRepository
{
    Task<List<TipoPersona>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<TipoPersona?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> TienePersonasAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(TipoPersona tipoPersona, CancellationToken cancellationToken = default);
    void Eliminar(TipoPersona tipoPersona);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
