using System.Security.Claims;
using AutoBrillo.Model.Reglas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoBrillo.Api.Seguridad;

/// <summary>
/// Roles de la operación del lavadero.
/// Administrador: todo, incluidas anulaciones, comisiones y el ABM.
/// Supervisor y Caja: lavados (alta, edición y cierre), ventas y cobros. No anulan.
/// Stock: compras, inventario y la lectura de personas y productos que esas pantallas necesitan.
/// </summary>
public static class RolesAutoBrillo
{
    public const string Administrador = "Administrador";
    public const string Supervisor = "Supervisor";
    public const string Caja = "Caja";
    public const string Stock = "Stock";
    public const string AdminStock = "Administrador,Stock";
    public const string Operacion = "Administrador,Supervisor,Caja";
    public const string LecturaCatalogo = "Administrador,Supervisor,Caja,Stock";
}

public static class ExtensionesUsuario
{
    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException();
}

/// <summary>Convierte una ReglaNegocioException en 400 { mensaje } para que la web muestre el motivo.</summary>
public class FiltroReglasNegocio : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ReglaNegocioException error)
        {
            context.Result = new BadRequestObjectResult(new { mensaje = error.Message });
            context.ExceptionHandled = true;
        }
    }
}
