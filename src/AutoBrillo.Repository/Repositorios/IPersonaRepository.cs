using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Repository.Repositorios;

public interface IPersonaRepository
{
    Task<List<Persona>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Persona?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken cancellationToken = default);
    Task AgregarAsync(Persona persona, CancellationToken cancellationToken = default);
    void Eliminar(Persona persona);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
