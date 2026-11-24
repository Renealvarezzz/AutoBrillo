using AutoBrillo.Api.Contratos;
using AutoBrillo.Api.Seguridad;
using AutoBrillo.Model.Entidades;
using AutoBrillo.Repository.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBrillo.Api.Controllers;

[ApiController, Authorize(Roles = RolesAutoBrillo.Operacion), Route("api")]
public class OperacionesController(IRepositorioSprintTres repo) : ControllerBase
{
    [HttpGet("lavados")]
    public async Task<IActionResult> Lavados(CancellationToken ct) => Ok((await repo.ObtenerLavadosAsync(ct)).Select(x => new {
        x.Id_Lavados, x.Id_Vehiculos, Vehiculo = x.Vehiculo!.Patente, Cliente = x.Vehiculo.Cliente!.Nombre,
        x.Sector, x.Fecha, x.FechaFinalizacion, x.Estado, x.MotivoAnulacion,
        Servicios = x.Servicios.Select(s => new { s.Id_Productos, Producto = s.Producto!.Descripcion, s.PrecioAplicado, s.EsAdicional }),
        Participantes = x.Participantes.Select(p => new { p.Id_Personas, Nombre = p.Persona!.Nombre }),
        HistorialEstados = x.HistorialEstados.OrderBy(h => h.Fecha).Select(h => new { h.Estado, h.Fecha, h.Id_Usuarios })
    }));

    [HttpPost("lavados")]
    [Authorize(Roles = RolesAutoBrillo.AdminCaja)]
    public async Task<IActionResult> RegistrarLavado(LavadoRequest r, CancellationToken ct)
    {
        var x = new Lavado { Id_Vehiculos = r.Id_Vehiculos, Sector = r.Sector, Fecha = r.Fecha,
            Servicios = (r.Id_Servicios ?? []).Select(id => new LavadoServicio { Id_Productos = id }).Concat((r.Id_Adicionales ?? []).Select(id => new LavadoServicio { Id_Productos = id, EsAdicional = true })).ToList(),
            Participantes = (r.Id_Participantes ?? []).Distinct().Select(id => new LavadoParticipante { Id_Personas = id }).ToList() };
        await repo.RegistrarLavadoAsync(x, User.IdUsuario(), ct); return Ok(new { x.Id_Lavados });
    }

    [HttpPut("lavados/{id:int}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> EditarLavado(int id, LavadoRequest r, CancellationToken ct)
    {
        var x = new Lavado { Id_Vehiculos = r.Id_Vehiculos, Sector = r.Sector, Fecha = r.Fecha,
            Servicios = (r.Id_Servicios ?? []).Select(i => new LavadoServicio { Id_Productos = i }).Concat((r.Id_Adicionales ?? []).Select(i => new LavadoServicio { Id_Productos = i, EsAdicional = true })).ToList(),
            Participantes = (r.Id_Participantes ?? []).Distinct().Select(i => new LavadoParticipante { Id_Personas = i }).ToList() };
        await repo.ActualizarLavadoAsync(id, x, ct); return NoContent();
    }

    [HttpPost("lavados/{id:int}/finalizar")]
    [Authorize(Roles = "Administrador,Supervisor,Caja")]
    public async Task<IActionResult> FinalizarLavado(int id, CancellationToken ct) { await repo.FinalizarLavadoAsync(id, User.IdUsuario(), ct); return NoContent(); }
    [HttpPost("lavados/{id:int}/cancelar"), Authorize(Roles = RolesAutoBrillo.Administrador)]
    public async Task<IActionResult> CancelarLavado(int id, AnulacionRequest r, CancellationToken ct) { await repo.AnularLavadoAsync(id, r.Motivo ?? "", User.IdUsuario(), ct); return NoContent(); }

    [HttpGet("ventas")]
    public async Task<IActionResult> Ventas(CancellationToken ct) => Ok((await repo.ObtenerVentasAsync(ct)).Select(x => new { x.Id_Ventas, x.Id_Lavados, x.Fecha, x.Condicion, Usuario = x.Usuario!.Nombre, x.Total, x.Estado, x.MotivoAnulacion,
        Cobrado = x.Cobros.Sum(c => c.Importe), x.Cobros, Detalles = x.Detalles.Select(d => new { d.Id_Productos, Producto = d.Producto!.Descripcion, d.Cantidad, d.PrecioUnitario, d.Subtotal }) }));
    [HttpPost("ventas")]
    public async Task<IActionResult> RegistrarVenta(VentaRequest r, CancellationToken ct)
    {
        var x = new Venta { Id_Lavados = r.Id_Lavados, Condicion = r.Condicion, Detalles = (r.Detalles ?? []).Select(d => new VentaDetalle { Id_Productos = d.Id_Productos, Cantidad = d.Cantidad }).ToList() };
        await repo.RegistrarVentaAsync(x, User.IdUsuario(), ct); return Ok(new { x.Id_Ventas, x.Total });
    }
    [HttpPost("ventas/{id:int}/cobros"), Authorize(Roles = RolesAutoBrillo.AdminCaja)]
    public async Task<IActionResult> Cobrar(int id, CobroRequest r, CancellationToken ct) { await repo.RegistrarCobroAsync(id, r.MedioPago, r.Importe, User.IdUsuario(), ct); return NoContent(); }
    [HttpPost("ventas/{id:int}/anular"), Authorize(Roles = RolesAutoBrillo.Administrador)]
    public async Task<IActionResult> AnularVenta(int id, AnulacionRequest r, CancellationToken ct) { await repo.AnularVentaAsync(id, r.Motivo ?? "", User.IdUsuario(), ct); return NoContent(); }

    [HttpGet("comisiones"), Authorize(Roles = RolesAutoBrillo.Administrador)]
    public async Task<IActionResult> Comisiones(CancellationToken ct) => Ok(new { porcentaje = await repo.ObtenerPorcentajeAsync(ct), items = (await repo.ObtenerComisionesAsync(ct)).Select(c => new { c.Id_Comisiones, c.Id_Ventas, Funcionario = c.Persona!.Nombre, c.PorcentajeAplicado, c.Importe, c.Anulada }) });
    [HttpPut("comisiones/porcentaje"), Authorize(Roles = RolesAutoBrillo.Administrador)]
    public async Task<IActionResult> Porcentaje(ConfiguracionComisionRequest r, CancellationToken ct) { await repo.GuardarPorcentajeAsync(r.Porcentaje, User.IdUsuario(), ct); return NoContent(); }
}
