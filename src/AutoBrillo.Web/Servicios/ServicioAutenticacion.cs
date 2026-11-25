using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace AutoBrillo.Web.Servicios;

/// <summary>Nombres de rol que la web usa para mostrar u ocultar pantallas.</summary>
public static class RolesPantalla
{
    public const string Administrador = "Administrador";
    public const string Supervisor = "Supervisor";
    public const string Caja = "Caja";
    public const string Stock = "Stock";
}

/// <summary>
/// Centraliza la comunicación de la web con los endpoints de autenticación de la API.
/// Así las páginas Login, Inicio y MainLayout no repiten solicitudes HTTP ni código de localStorage.
/// </summary>
public class ServicioAutenticacion(HttpClient http, IJSRuntime js)
{
    private const string ClaveToken = "autobrillo.token";
    private const string ClaveUsuario = "autobrillo.usuario";
    private const string ClaveEsAdministrador = "autobrillo.esAdministrador";
    private const string ClaveRoles = "autobrillo.roles";
    private const string ClaveId = "autobrillo.idUsuario";

    /// <summary>
    /// Envía Nombre y Password a POST /api/auth/login.
    /// Si la API acepta las credenciales, guarda el token, el nombre y los roles.
    /// </summary>
    /// <returns>Null cuando el acceso fue correcto; un mensaje cuando falló.</returns>
    public async Task<string?> LoginAsync(string nombre, string password)
    {
        var respuesta = await http.PostAsJsonAsync("api/auth/login", new { nombre, password });
        if (!respuesta.IsSuccessStatusCode)
            return "Nombre o contraseña incorrectos.";

        var datos = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        if (datos is null) return "No fue posible leer la respuesta de la API.";

        await GuardarSesionAsync(datos.Token, datos.Usuario, datos.EsAdministrador, datos.Roles, null);
        return null;
    }

    /// <summary>Obtiene el nombre almacenado para mostrarlo en la barra superior.</summary>
    public async Task<string?> ObtenerUsuarioAsync() =>
        await js.InvokeAsync<string?>("localStorage.getItem", ClaveUsuario);

    /// <summary>Identificador del usuario de la sesión, o 0 si todavía no se guardó.</summary>
    public async Task<int> ObtenerIdUsuarioAsync()
    {
        var valor = await js.InvokeAsync<string?>("localStorage.getItem", ClaveId);
        return int.TryParse(valor, out var id) ? id : 0;
    }

    /// <summary>Roles guardados al iniciar sesión o al validarla.</summary>
    public async Task<IReadOnlyList<string>> ObtenerRolesAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", ClaveRoles);
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var lista = JsonSerializer.Deserialize<List<string>>(json);
                if (lista is { Count: > 0 }) return lista;
            }
            catch (JsonException) { }
        }

        var flag = await js.InvokeAsync<string?>("localStorage.getItem", ClaveEsAdministrador);
        return bool.TryParse(flag, out var esAdministrador) && esAdministrador ? [RolesPantalla.Administrador] : [];
    }

    /// <summary>Indica si el usuario actual posee el rol Administrador.</summary>
    public async Task<bool> EsAdministradorAsync() =>
        (await ObtenerRolesAsync()).Contains(RolesPantalla.Administrador, StringComparer.OrdinalIgnoreCase);

    /// <summary>Verdadero si la sesión tiene al menos uno de los roles indicados.</summary>
    public async Task<bool> TieneAlgunRolAsync(params string[] requeridos)
    {
        var actuales = await ObtenerRolesAsync();
        return requeridos.Any(rol => actuales.Contains(rol, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Comprueba que existe un token y pregunta a GET /api/auth/me si sigue siendo válido.
    /// Devuelve false cuando no hay sesión, el token venció o la API lo rechaza.
    /// </summary>
    public async Task<bool> ValidarSesionAsync()
    {
        var token = await js.InvokeAsync<string?>("localStorage.getItem", ClaveToken);
        if (string.IsNullOrWhiteSpace(token)) return false;

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var respuesta = await http.GetAsync("api/auth/me");
        if (!respuesta.IsSuccessStatusCode) return false;

        var datos = await respuesta.Content.ReadFromJsonAsync<MeResponse>();
        if (datos is not null)
            await GuardarIdentidadAsync(datos.EsAdministrador, datos.Roles, datos.Id);
        return true;
    }

    /// <summary>
    /// Elimina los datos guardados en el navegador y deja de enviar el token en solicitudes futuras.
    /// </summary>
    public async Task CerrarSesionAsync()
    {
        http.DefaultRequestHeaders.Authorization = null;
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveToken);
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveUsuario);
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveEsAdministrador);
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveRoles);
        await js.InvokeVoidAsync("localStorage.removeItem", ClaveId);
    }

    private async Task GuardarSesionAsync(string token, string usuario, bool esAdministrador, IEnumerable<string>? roles, string? id)
    {
        await js.InvokeVoidAsync("localStorage.setItem", ClaveToken, token);
        await js.InvokeVoidAsync("localStorage.setItem", ClaveUsuario, usuario);
        await GuardarIdentidadAsync(esAdministrador, roles, id);
    }

    private async Task GuardarIdentidadAsync(bool esAdministrador, IEnumerable<string>? roles, string? id)
    {
        var lista = roles?.Where(rol => !string.IsNullOrWhiteSpace(rol)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (esAdministrador && !lista.Contains(RolesPantalla.Administrador, StringComparer.OrdinalIgnoreCase))
            lista.Add(RolesPantalla.Administrador);
        var esAdmin = lista.Contains(RolesPantalla.Administrador, StringComparer.OrdinalIgnoreCase);
        await js.InvokeVoidAsync("localStorage.setItem", ClaveEsAdministrador, esAdmin.ToString().ToLowerInvariant());
        await js.InvokeVoidAsync("localStorage.setItem", ClaveRoles, JsonSerializer.Serialize(lista));
        if (!string.IsNullOrWhiteSpace(id))
            await js.InvokeVoidAsync("localStorage.setItem", ClaveId, id);
    }

    private sealed record LoginResponse(string Token, string Usuario, bool EsAdministrador, List<string>? Roles);

    private sealed record MeResponse(string? Id, bool EsAdministrador, List<string>? Roles);
}
