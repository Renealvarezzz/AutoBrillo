using System.Security.Claims;
using AutoBrillo.Model.Reglas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoBrillo.Api.Seguridad;

/// <summary>Roles ya cargados en la tabla roles.</summary>
public static class RolesAutoBrillo
{
    public const string Administrador = "Administrador";
    public const string AdminStock = "Administrador,Stock";
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
