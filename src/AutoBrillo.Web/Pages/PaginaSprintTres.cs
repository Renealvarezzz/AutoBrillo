using AutoBrillo.Web.Servicios;

namespace AutoBrillo.Web.Pages;

/// <summary>Lavados, ventas y el resto de la operación diaria: administrador, supervisor y caja.</summary>
public abstract class PaginaSprintTres : PaginaOperativa
{
    protected override string[] RolesPermitidos => [RolesPantalla.Administrador, RolesPantalla.Supervisor, RolesPantalla.Caja];
}
