using AutoBrillo.Model.Entidades;

namespace AutoBrillo.Repository.Repositorios;

/// <summary>Contrato de consultas y cambios sobre los roles.</summary>
public interface IRolRepository
{
    Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Rol?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Rol?> ObtenerPorDescripcionAsync(string descripcion, CancellationToken cancellationToken = default);
    Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> TieneUsuariosAsync(int id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Rol rol, CancellationToken cancellationToken = default);
    void Eliminar(Rol rol);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
