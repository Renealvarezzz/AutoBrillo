using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public interface IRepositorioVehiculos
{
    Task<List<Vehiculo>> ObtenerAsync(int? idPersona, CancellationToken ct = default);
    Task<Vehiculo> GuardarAsync(Vehiculo vehiculo, CancellationToken ct = default);
    Task<Vehiculo> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default);
}

/// <summary>PBI-004: vehículos pertenecientes a un cliente.</summary>
public class RepositorioVehiculos(AutoBrilloDbContext contexto, ServicioOperativo operativo) : IRepositorioVehiculos
{
    public Task<List<Vehiculo>> ObtenerAsync(int? idPersona, CancellationToken ct = default) =>
        contexto.Vehiculos.AsNoTracking().Include(x => x.Cliente).Include(x => x.TipoVehiculo)
            .Where(x => idPersona == null || x.Id_Personas == idPersona)
            .OrderBy(x => x.Cliente!.Nombre).ThenBy(x => x.Patente).ToListAsync(ct);

    public async Task<Vehiculo> GuardarAsync(Vehiculo vehiculo, CancellationToken ct = default)
    {
        vehiculo.Patente = vehiculo.Patente?.Trim().ToUpperInvariant() ?? string.Empty;
        if (vehiculo.Patente.Length is 0 or > 20) throw new ReglaNegocioException("Ingresá la patente o chapa del vehículo (hasta 20 caracteres).");
        vehiculo.Marca = Limpiar(vehiculo.Marca, 80);
        vehiculo.Modelo = Limpiar(vehiculo.Modelo, 80);
        vehiculo.Color = Limpiar(vehiculo.Color, 40);

        if (!await operativo.PersonaTieneTipoAsync(vehiculo.Id_Personas, "Cliente", ct))
            throw new ReglaNegocioException("Seleccioná un cliente activo.");
        if (!await contexto.TiposVehiculos.AnyAsync(x => x.Id_Tipos_Vehiculos == vehiculo.Id_Tipos_Vehiculos && x.Activo, ct))
            throw new ReglaNegocioException("Seleccioná un tipo de vehículo activo.");
        if (await contexto.Vehiculos.AnyAsync(x => x.Patente == vehiculo.Patente && x.Id_Vehiculos != vehiculo.Id_Vehiculos, ct))
            throw new ReglaNegocioException("Ya existe un vehículo con esa patente.");

        Vehiculo guardado;
        if (vehiculo.Id_Vehiculos == 0)
        {
            guardado = vehiculo;
            contexto.Vehiculos.Add(guardado);
        }
        else
        {
            guardado = await contexto.Vehiculos.FirstOrDefaultAsync(x => x.Id_Vehiculos == vehiculo.Id_Vehiculos, ct)
                ?? throw new ReglaNegocioException("Vehículo no encontrado.");
            guardado.Id_Personas = vehiculo.Id_Personas;
            guardado.Id_Tipos_Vehiculos = vehiculo.Id_Tipos_Vehiculos;
            guardado.Patente = vehiculo.Patente;
            guardado.Marca = vehiculo.Marca;
            guardado.Modelo = vehiculo.Modelo;
            guardado.Color = vehiculo.Color;
            guardado.Activo = vehiculo.Activo;
        }

        await contexto.SaveChangesAsync(ct);
        return guardado;
    }

    public async Task<Vehiculo> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        var vehiculo = await contexto.Vehiculos.FirstOrDefaultAsync(x => x.Id_Vehiculos == id, ct)
            ?? throw new ReglaNegocioException("Vehículo no encontrado.");
        vehiculo.Activo = activo;
        await contexto.SaveChangesAsync(ct);
        return vehiculo;
    }

    private static string? Limpiar(string? texto, int maximo)
    {
        var valor = texto?.Trim();
        if (string.IsNullOrEmpty(valor)) return null;
        return valor.Length > maximo ? valor[..maximo] : valor;
    }
}
