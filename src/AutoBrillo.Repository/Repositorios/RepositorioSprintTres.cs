using AutoBrillo.Model.Entidades;
using AutoBrillo.Model.Reglas;
using AutoBrillo.Repository.Datos;
using Microsoft.EntityFrameworkCore;

namespace AutoBrillo.Repository.Repositorios;

public interface IRepositorioSprintTres
{
    Task<List<Lavado>> ObtenerLavadosAsync(CancellationToken ct);
    Task<Lavado> RegistrarLavadoAsync(Lavado lavado, int usuario, CancellationToken ct);
    Task<Lavado> ActualizarLavadoAsync(int id, Lavado entrada, CancellationToken ct);
    Task FinalizarLavadoAsync(int id, int usuario, CancellationToken ct);
    Task AnularLavadoAsync(int id, string motivo, int usuario, CancellationToken ct);
    Task<List<Venta>> ObtenerVentasAsync(CancellationToken ct);
    Task<Venta> RegistrarVentaAsync(Venta venta, int usuario, CancellationToken ct);
    Task RegistrarCobroAsync(int id, string medio, decimal importe, int usuario, CancellationToken ct);
    Task AnularVentaAsync(int id, string motivo, int usuario, CancellationToken ct);
    Task<decimal> ObtenerPorcentajeAsync(CancellationToken ct);
    Task GuardarPorcentajeAsync(decimal porcentaje, int usuario, CancellationToken ct);
    Task<List<Comision>> ObtenerComisionesAsync(CancellationToken ct);
}

public class RepositorioSprintTres(AutoBrilloDbContext db, ServicioOperativo operativo) : IRepositorioSprintTres
{
    public Task<List<Lavado>> ObtenerLavadosAsync(CancellationToken ct) => db.Lavados.AsNoTracking()
        .Include(x => x.Vehiculo).ThenInclude(x => x!.Cliente).Include(x => x.Servicios).ThenInclude(x => x.Producto)
        .Include(x => x.Participantes).ThenInclude(x => x.Persona).Include(x => x.HistorialEstados).OrderByDescending(x => x.Fecha).ToListAsync(ct);

    public async Task<Lavado> RegistrarLavadoAsync(Lavado lavado, int usuario, CancellationToken ct)
    {
        lavado.Fecha = ServicioOperativo.NormalizarFecha(lavado.Fecha);
        var vehiculo = await db.Vehiculos.Include(x => x.Cliente).FirstOrDefaultAsync(x => x.Id_Vehiculos == lavado.Id_Vehiculos && x.Activo && x.Cliente!.Activo, ct)
            ?? throw new ReglaNegocioException("Seleccioná un vehículo registrado y activo.");
        if (lavado.Sector is < 1 or > 10) throw new ReglaNegocioException("Seleccioná uno de los diez sectores.");
        var empleados = lavado.Participantes.Select(x => x.Id_Personas).Distinct().ToList();
        var empleadosValidos = await db.Personas.CountAsync(p => empleados.Contains(p.Id_Personas) && p.Activo && p.Tipos.Any(t => t.Descripcion == "Funcionario"), ct);
        if (empleados.Count == 0 || empleadosValidos != empleados.Count)
            throw new ReglaNegocioException("Seleccioná uno o más funcionarios activos.");
        if (lavado.Servicios.Count == 0 || lavado.Servicios.Count(x => !x.EsAdicional) != 1 || lavado.Servicios.Select(x => x.Id_Productos).Distinct().Count() != lavado.Servicios.Count) throw new ReglaNegocioException("El lavado debe incluir un servicio base y servicios sin duplicados.");
        if (await db.Lavados.AnyAsync(x => x.Sector == lavado.Sector && x.Fecha == lavado.Fecha && x.Estado == "Registrado", ct)) throw new ReglaNegocioException("Ese sector ya está ocupado en ese horario.");
        var tipoVehiculo = vehiculo.Id_Tipos_Vehiculos;
        foreach (var linea in lavado.Servicios)
        {
            var producto = await operativo.ObtenerProductoAsync(linea.Id_Productos, ct);
            if (!producto.Activo) throw new ReglaNegocioException("No se puede agregar un servicio inactivo.");
            if (linea.EsAdicional && !ServicioOperativo.TieneTipo(producto, ServicioOperativo.Adicional)) throw new ReglaNegocioException($"«{producto.Descripcion}» no es un adicional.");
            if (!linea.EsAdicional && !ServicioOperativo.TieneTipo(producto, ServicioOperativo.Servicio)) throw new ReglaNegocioException("Elegí un servicio activo.");
            linea.PrecioAplicado = await operativo.ResolverPrecioAsync(linea.Id_Productos, tipoVehiculo, ct);
        }
        lavado.Estado = "Registrado"; lavado.Id_Usuarios = usuario;
        lavado.HistorialEstados.Add(new LavadoHistorialEstado { Estado = lavado.Estado, Fecha = ServicioOperativo.Ahora(), Id_Usuarios = usuario });
        db.Lavados.Add(lavado); await db.SaveChangesAsync(ct); return lavado;
    }

    public async Task<Lavado> ActualizarLavadoAsync(int id, Lavado entrada, CancellationToken ct)
    {
        entrada.Fecha = ServicioOperativo.NormalizarFecha(entrada.Fecha);
        var lavado = await db.Lavados.Include(x => x.Servicios).Include(x => x.Participantes).FirstOrDefaultAsync(x => x.Id_Lavados == id, ct)
            ?? throw new ReglaNegocioException("Lavado no encontrado.");
        if (lavado.Estado != "Registrado") throw new ReglaNegocioException("Solo se pueden modificar lavados registrados.");
        if (await db.Ventas.AnyAsync(v => v.Id_Lavados == id && v.Estado != "Anulada", ct)) throw new ReglaNegocioException("No se puede modificar un lavado con una venta activa.");
        var vehiculo = await db.Vehiculos.FirstOrDefaultAsync(x => x.Id_Vehiculos == entrada.Id_Vehiculos && x.Activo && x.Cliente!.Activo, ct)
            ?? throw new ReglaNegocioException("Seleccioná un vehículo registrado y activo.");
        if (entrada.Sector is < 1 or > 10) throw new ReglaNegocioException("Seleccioná uno de los diez sectores.");
        if (await db.Lavados.AnyAsync(x => x.Id_Lavados != id && x.Sector == entrada.Sector && x.Fecha == entrada.Fecha && x.Estado == "Registrado", ct)) throw new ReglaNegocioException("Ese sector ya está ocupado en ese horario.");
        var empleados = entrada.Participantes.Select(x => x.Id_Personas).Distinct().ToList();
        if (empleados.Count == 0 || await db.Personas.CountAsync(p => empleados.Contains(p.Id_Personas) && p.Activo && p.Tipos.Any(t => t.Descripcion == "Funcionario"), ct) != empleados.Count)
            throw new ReglaNegocioException("Seleccioná uno o más funcionarios activos.");
        if (entrada.Servicios.Count == 0 || entrada.Servicios.Count(x => !x.EsAdicional) != 1 || entrada.Servicios.Select(x => x.Id_Productos).Distinct().Count() != entrada.Servicios.Count)
            throw new ReglaNegocioException("El lavado debe incluir un servicio base y servicios sin duplicados.");
        foreach (var linea in entrada.Servicios)
        {
            var producto = await operativo.ObtenerProductoAsync(linea.Id_Productos, ct);
            if (!producto.Activo || (linea.EsAdicional ? !ServicioOperativo.TieneTipo(producto, ServicioOperativo.Adicional) : !ServicioOperativo.TieneTipo(producto, ServicioOperativo.Servicio)))
                throw new ReglaNegocioException("Revisá el servicio y los adicionales seleccionados.");
            linea.PrecioAplicado = await operativo.ResolverPrecioAsync(linea.Id_Productos, vehiculo.Id_Tipos_Vehiculos, ct);
        }
        db.LavadosServicios.RemoveRange(lavado.Servicios); db.LavadosParticipantes.RemoveRange(lavado.Participantes);
        lavado.Id_Vehiculos = entrada.Id_Vehiculos; lavado.Sector = entrada.Sector; lavado.Fecha = entrada.Fecha;
        lavado.Servicios = entrada.Servicios; lavado.Participantes = entrada.Participantes;
        await db.SaveChangesAsync(ct); return lavado;
    }

    public async Task FinalizarLavadoAsync(int id, int usuario, CancellationToken ct)
    {
        var lavado = await db.Lavados.Include(x => x.Servicios).FirstOrDefaultAsync(x => x.Id_Lavados == id, ct) ?? throw new ReglaNegocioException("Lavado no encontrado.");
        if (lavado.Estado != "Registrado") throw new ReglaNegocioException("El lavado ya no está registrado.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        foreach (var linea in lavado.Servicios)
        {
            var consumo = await db.ServiciosInsumos.AsNoTracking().Where(x => x.Id_Servicios == linea.Id_Productos).ToListAsync(ct);
            foreach (var insumo in consumo)
                await operativo.MoverStockAsync(insumo.Id_Insumos, ReglasNegocio.Salida, "Lavado", insumo.Cantidad, id, linea.Id_Productos.ToString(), usuario, ct, forzar: true);
        }
        lavado.Estado = "Terminado"; lavado.FechaFinalizacion = ServicioOperativo.Ahora();
        lavado.HistorialEstados.Add(new LavadoHistorialEstado { Id_Lavados = id, Estado = lavado.Estado, Fecha = lavado.FechaFinalizacion.Value, Id_Usuarios = usuario });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }

    public async Task AnularLavadoAsync(int id, string motivo, int usuario, CancellationToken ct)
    {
        var lavado = await db.Lavados.Include(x => x.Servicios).FirstOrDefaultAsync(x => x.Id_Lavados == id, ct) ?? throw new ReglaNegocioException("Lavado no encontrado.");
        if (lavado.Estado is not ("Registrado" or "Terminado")) throw new ReglaNegocioException("Solo se puede cancelar un lavado registrado o terminado.");
        var texto = ServicioOperativo.ValidarMotivo(motivo);
        var estabaTerminado = lavado.Estado == "Terminado";
        var ventas = await db.Ventas.Include(x => x.Detalles).Include(x => x.Comisiones)
            .Where(x => x.Id_Lavados == id && x.Estado == "Registrada").ToListAsync(ct);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        ServicioOperativo.MarcarAnulada(lavado, "Cancelado", texto, usuario);
        if (estabaTerminado)
            foreach (var linea in lavado.Servicios)
                foreach (var insumo in await db.ServiciosInsumos.AsNoTracking().Where(x => x.Id_Servicios == linea.Id_Productos).ToListAsync(ct))
                    await operativo.MoverStockAsync(insumo.Id_Insumos, ReglasNegocio.Entrada, "Anulación lavado", insumo.Cantidad, id, texto, usuario, ct, forzar: true);
        lavado.HistorialEstados.Add(new LavadoHistorialEstado { Id_Lavados = id, Estado = lavado.Estado, Fecha = ServicioOperativo.Ahora(), Id_Usuarios = usuario });
        foreach (var venta in ventas) await AplicarAnulacionVentaAsync(venta, texto, usuario, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public Task<List<Venta>> ObtenerVentasAsync(CancellationToken ct) => db.Ventas.AsNoTracking().Include(x => x.Usuario).Include(x => x.Lavado).Include(x => x.Detalles).ThenInclude(x => x.Producto).Include(x => x.Cobros).OrderByDescending(x => x.Fecha).ToListAsync(ct);

    public async Task<Venta> RegistrarVentaAsync(Venta venta, int usuario, CancellationToken ct)
    {
        if (venta.Condicion is not ("Contado" or "Crédito")) throw new ReglaNegocioException("La condición debe ser Contado o Crédito.");
        if (venta.Detalles.Count == 0) throw new ReglaNegocioException("Agregá al menos un producto o servicio.");
        Lavado? lavadoAsociado = null;
        if (venta.Id_Lavados is int idLavadoAsociado)
        {
            lavadoAsociado = await db.Lavados.Include(x => x.Servicios).FirstOrDefaultAsync(x => x.Id_Lavados == idLavadoAsociado, ct)
                ?? throw new ReglaNegocioException("El lavado asociado no está disponible.");
            var tieneVentaActiva = await db.Ventas.AnyAsync(v => v.Id_Lavados == idLavadoAsociado && v.Estado != "Anulada", ct);
            if (!ReglasNegocio.LavadoDisponibleParaVenta(lavadoAsociado.Estado, tieneVentaActiva))
                throw new ReglaNegocioException(tieneVentaActiva ? "Ese lavado ya tiene una venta activa." : "El lavado asociado no está disponible.");
            var servicioLavado = lavadoAsociado.Servicios.Single(x => !x.EsAdicional);
            var lineasServicio = venta.Detalles.Where(x => x.Id_Productos == servicioLavado.Id_Productos).ToList();
            if (lineasServicio.Count != 1 || lineasServicio[0].Cantidad != 1)
                throw new ReglaNegocioException("La venta asociada debe incluir el servicio del lavado.");
        }
        var subtotalesComision = new List<decimal>();
        foreach (var d in venta.Detalles)
        {
            if (d.Cantidad <= 0 || d.Cantidad != decimal.Truncate(d.Cantidad)) throw new ReglaNegocioException("La cantidad debe ser un entero mayor a cero.");
            var p = await operativo.ObtenerProductoAsync(d.Id_Productos, ct);
            if (!p.Activo) throw new ReglaNegocioException($"«{p.Descripcion}» está inactivo.");
            var aplicado = lavadoAsociado?.Servicios.FirstOrDefault(x => x.Id_Productos == d.Id_Productos);
            d.PrecioUnitario = aplicado?.PrecioAplicado ?? p.PrecioDeVenta;
            if (d.PrecioUnitario < 0 || d.PrecioUnitario != decimal.Truncate(d.PrecioUnitario)) throw new ReglaNegocioException("El precio del producto debe ser un entero no negativo.");
            d.Subtotal = d.Cantidad * d.PrecioUnitario;
            if (ServicioOperativo.TieneTipo(p, ServicioOperativo.Servicio) || ServicioOperativo.TieneTipo(p, ServicioOperativo.Adicional))
                subtotalesComision.Add(d.Subtotal);
        }
        venta.Fecha = ServicioOperativo.Ahora(); venta.Estado = "Registrada"; venta.Id_Usuarios = usuario; venta.Total = venta.Detalles.Sum(x => x.Subtotal);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            db.Ventas.Add(venta); await db.SaveChangesAsync(ct);
            if (venta.Id_Lavados is int idLavadoComision)
            {
                var lavadoParaComision = await db.Lavados.Include(x => x.Participantes).FirstAsync(x => x.Id_Lavados == idLavadoComision, ct);
                var porcentaje = await ObtenerPorcentajeAsync(ct);
                var importe = ReglasNegocio.CalcularComision(ReglasNegocio.SumarBaseComision(subtotalesComision), porcentaje, lavadoParaComision.Participantes.Count);
                db.Comisiones.AddRange(lavadoParaComision.Participantes.Select(x => new Comision { Id_Ventas = venta.Id_Ventas, Id_Personas = x.Id_Personas, PorcentajeAplicado = porcentaje, Importe = importe }));
            }
            foreach (var d in venta.Detalles)
                if (ServicioOperativo.ManejaStock(await operativo.ObtenerProductoAsync(d.Id_Productos, ct))) await operativo.MoverStockAsync(d.Id_Productos, ReglasNegocio.Salida, "Venta", d.Cantidad, venta.Id_Ventas, null, usuario, ct);
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return venta;
        }
        catch (DbUpdateException ex) when (EsVentaDuplicada(ex))
        {
            throw new ReglaNegocioException("Ese lavado ya tiene una venta activa.");
        }
    }

    public async Task RegistrarCobroAsync(int id, string medio, decimal importe, int usuario, CancellationToken ct)
    {
        if (medio is not ("Efectivo" or "Débito" or "Crédito")) throw new ReglaNegocioException("Medio de pago inválido.");
        if (importe <= 0 || importe != decimal.Truncate(importe)) throw new ReglaNegocioException("El importe del cobro debe ser un entero mayor a cero.");
        var venta = await db.Ventas.Include(x => x.Cobros).FirstOrDefaultAsync(x => x.Id_Ventas == id && x.Estado == "Registrada", ct) ?? throw new ReglaNegocioException("Venta no encontrada o anulada.");
        if (importe + venta.Cobros.Sum(x => x.Importe) > venta.Total) throw new ReglaNegocioException("El cobro supera el saldo de la venta.");
        db.Cobros.Add(new Cobro { Id_Ventas = id, Fecha = ServicioOperativo.Ahora(), MedioPago = medio, Importe = importe, Id_Usuarios = usuario }); await db.SaveChangesAsync(ct);
    }

    public async Task AnularVentaAsync(int id, string motivo, int usuario, CancellationToken ct)
    {
        var venta = await db.Ventas.Include(x => x.Detalles).Include(x => x.Comisiones).FirstOrDefaultAsync(x => x.Id_Ventas == id, ct) ?? throw new ReglaNegocioException("Venta no encontrada.");
        var texto = ServicioOperativo.ValidarMotivo(motivo);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await AplicarAnulacionVentaAsync(venta, texto, usuario, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task AplicarAnulacionVentaAsync(Venta venta, string motivo, int usuario, CancellationToken ct)
    {
        ServicioOperativo.MarcarAnulada(venta, "Anulada", motivo, usuario);
        foreach (var c in venta.Comisiones) c.Anulada = true;
        foreach (var d in venta.Detalles)
            if (ServicioOperativo.ManejaStock(await operativo.ObtenerProductoAsync(d.Id_Productos, ct))) await operativo.MoverStockAsync(d.Id_Productos, ReglasNegocio.Entrada, "Anulación venta", d.Cantidad, venta.Id_Ventas, motivo, usuario, ct, forzar: true);
    }

    private static bool EsVentaDuplicada(DbUpdateException ex) =>
        ex.InnerException?.Message.Contains("ux_ventas_lavado_activa", StringComparison.OrdinalIgnoreCase) == true;

    public async Task<decimal> ObtenerPorcentajeAsync(CancellationToken ct) => await db.ConfiguracionesComision.Select(x => (decimal?)x.Porcentaje).FirstOrDefaultAsync(ct) ?? 30m;
    public async Task GuardarPorcentajeAsync(decimal porcentaje, int usuario, CancellationToken ct)
    {
        if (porcentaje < 0 || porcentaje > 100) throw new ReglaNegocioException("El porcentaje debe estar entre 0 y 100.");
        var cfg = await db.ConfiguracionesComision.FirstOrDefaultAsync(ct);
        if (cfg is null) db.ConfiguracionesComision.Add(new() { Id_Configuracion_Comision = 1, Porcentaje = porcentaje, Id_Usuarios = usuario, FechaActualizacion = ServicioOperativo.Ahora() });
        else { cfg.Porcentaje = porcentaje; cfg.Id_Usuarios = usuario; cfg.FechaActualizacion = ServicioOperativo.Ahora(); }
        await db.SaveChangesAsync(ct);
    }
    public Task<List<Comision>> ObtenerComisionesAsync(CancellationToken ct) => db.Comisiones.AsNoTracking().Include(x => x.Persona).Include(x => x.Venta).OrderByDescending(x => x.Id_Comisiones).ToListAsync(ct);
}
