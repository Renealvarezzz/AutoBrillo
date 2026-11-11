using AutoBrillo.Api.Contratos;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

/// <summary>PBI-004: vehículos de los clientes y tipos de vehículo.</summary>
[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api")]
public class VehiculosController(IRepositorioVehiculos vehiculos, IRepositorioCatalogos catalogos) : ControllerBase
{
    [HttpGet("vehiculos")]
    public async Task<IActionResult> Obtener([FromQuery] int? idPersona, CancellationToken ct) =>
        Ok((await vehiculos.ObtenerAsync(idPersona, ct)).Select(Convertir));

    [HttpPost("vehiculos")]
    public async Task<IActionResult> Crear(VehiculoRequest request, CancellationToken ct)
    {
        var guardado = await vehiculos.GuardarAsync(Desde(0, request), ct);
        return Ok(new { guardado.Id_Vehiculos });
    }

    [HttpPut("vehiculos/{id:int}")]
    public async Task<IActionResult> Editar(int id, VehiculoRequest request, CancellationToken ct)
    {
        var guardado = await vehiculos.GuardarAsync(Desde(id, request), ct);
        return Ok(new { guardado.Id_Vehiculos });
    }

    [HttpPatch("vehiculos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, EstadoVehiculoRequest request, CancellationToken ct)
    {
        var guardado = await vehiculos.CambiarEstadoAsync(id, request.Activo, ct);
        return Ok(new { guardado.Id_Vehiculos, guardado.Activo });
    }

    [HttpGet("tipos-vehiculos")]
    public async Task<IActionResult> Tipos(CancellationToken ct) => Ok(await catalogos.ObtenerTiposVehiculosAsync(ct));

    [HttpPost("tipos-vehiculos")]
    public async Task<IActionResult> CrearTipo(CatalogoRequest request, CancellationToken ct) =>
        Ok(await catalogos.GuardarTipoVehiculoAsync(0, request.Descripcion, request.Activo, ct));

    [HttpPut("tipos-vehiculos/{id:int}")]
    public async Task<IActionResult> EditarTipo(int id, CatalogoRequest request, CancellationToken ct) =>
        Ok(await catalogos.GuardarTipoVehiculoAsync(id, request.Descripcion, request.Activo, ct));

    private static Vehiculo Desde(int id, VehiculoRequest request) => new()
    {
        Id_Vehiculos = id,
        Id_Personas = request.Id_Personas,
        Id_Tipos_Vehiculos = request.Id_Tipos_Vehiculos,
        Patente = request.Patente,
        Marca = request.Marca,
        Modelo = request.Modelo,
        Color = request.Color,
        Activo = request.Activo
    };

    private static object Convertir(Vehiculo v) => new
    {
        v.Id_Vehiculos,
        v.Id_Personas,
        Cliente = v.Cliente?.Nombre,
        v.Id_Tipos_Vehiculos,
        TipoVehiculo = v.TipoVehiculo?.Descripcion,
        v.Patente,
        v.Marca,
        v.Modelo,
        v.Color,
        v.Activo
    };
}
