using System.Net.Http.Json;

namespace AutoBrillo.Web.Servicios;

/// <summary>HTTP/JSON de vehículos, servicios, inventario y compras.</summary>
public class ServicioOperaciones(HttpClient http)
{
    public async Task<T?> ObtenerAsync<T>(string url)
    {
        try { return await http.GetFromJsonAsync<T>(url); }
        catch (HttpRequestException) { return default; }
    }

    public Task<string?> PostAsync(string url, object datos) => EnviarAsync(HttpMethod.Post, url, datos);
    public Task<string?> PutAsync(string url, object datos) => EnviarAsync(HttpMethod.Put, url, datos);
    public Task<string?> PatchAsync(string url, object datos) => EnviarAsync(HttpMethod.Patch, url, datos);
    public Task<string?> DeleteAsync(string url) => EnviarAsync(HttpMethod.Delete, url, null);

    public async Task<string?> EnviarAsync(HttpMethod metodo, string url, object? datos)
    {
        using var solicitud = new HttpRequestMessage(metodo, url) { Content = datos is null ? null : JsonContent.Create(datos) };
        return await LeerErrorAsync(await http.SendAsync(solicitud));
    }

    private static async Task<string?> LeerErrorAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode) return null;
        if (respuesta.StatusCode == System.Net.HttpStatusCode.Forbidden) return "Tu usuario no tiene permiso para esta acción.";
        try
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<ProblemaApi>();
            if (!string.IsNullOrWhiteSpace(problema?.Mensaje)) return problema.Mensaje;
            if (problema?.Errors is not null) return problema.Errors.Values.SelectMany(x => x).FirstOrDefault() ?? "No fue posible completar la operación.";
        }
        catch (Exception) { }
        return "No fue posible completar la operación. Revisá los datos e intentá de nuevo.";
    }

    private sealed class ProblemaApi
    {
        public string? Mensaje { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
