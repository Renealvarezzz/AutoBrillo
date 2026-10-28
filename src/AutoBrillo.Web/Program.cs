using AutoBrillo.Web;
using AutoBrillo.Web.Servicios;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Punto de entrada de la aplicación web Blazor que corre en el navegador.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Indica dónde Blazor debe dibujar la aplicación y los cambios del encabezado HTML.
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Lee la dirección de la API desde appsettings.json al iniciar la web.
// La web conoce solo esta URL; nunca conoce la cadena de conexión de PostgreSQL.
var apiUrl = builder.Configuration["ApiUrl"] ?? "https://localhost:7001/";

// HttpClient es el objeto que permite enviar solicitudes HTTP/JSON a la API.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiUrl) });

// Registra el servicio que concentra las acciones de login, validación y cierre de sesión.
builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioGestion>();
builder.Services.AddScoped<ServicioAlertas>();

// Construye e inicia la aplicación en el navegador.
await builder.Build().RunAsync();
