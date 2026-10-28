using System.Security.Claims;
using System.Text;
using AutoBrillo.Repository.Datos;
using AutoBrillo.Repository.Repositorios;
using AutoBrillo.Repository.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Punto de entrada de la API ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// El hosting puede indicar el puerto con PORT. Docker usa esta variable para escuchar en 8080.
var puerto = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puerto))
    builder.WebHost.UseUrls($"http://0.0.0.0:{puerto}");

// La configuración sensible se obtiene del entorno, nunca se escribe en el código ni en GitHub.
var cadenaConexion = builder.Configuration["CONNECTION_STRING"]
    ?? throw new InvalidOperationException("Falta la variable de entorno CONNECTION_STRING.");
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("Falta la variable de entorno JWT_SECRET.");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "AutoBrillo.Api";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "AutoBrillo.Web";

// Registra la conexión a PostgreSQL y las clases que usa el controlador por inyección de dependencias.
builder.Services.AddDbContext<AutoBrilloDbContext>(options => options.UseNpgsql(cadenaConexion));
builder.Services.AddScoped<IUserRepository, RepositorioUsuarios>();
builder.Services.AddScoped<IRolRepository, RepositorioRoles>();
builder.Services.AddScoped<IPersonaRepository, RepositorioPersonas>();
builder.Services.AddScoped<ITipoPersonaRepository, RepositorioTiposPersonas>();
builder.Services.AddScoped<IProductoRepository, RepositorioProductos>();
builder.Services.AddScoped<ITipoProductoRepository, RepositorioTiposProductos>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddControllers();

// CORS define qué sitio web puede llamar a la API desde un navegador.
// En producción WEB_ORIGIN contiene la URL pública de la web de Vercel.
var origenWeb = builder.Configuration["WEB_ORIGIN"];
builder.Services.AddCors(options => options.AddPolicy("Web", policy =>
{
    policy.AllowAnyHeader().AllowAnyMethod();
    if (string.IsNullOrWhiteSpace(origenWeb))
        policy.AllowAnyOrigin(); // Solo sirve para facilitar pruebas locales.
    else
        policy.WithOrigins(origenWeb);
}));

// Configura cómo ASP.NET Core valida los tokens JWT que llegan en Authorization: Bearer TOKEN.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    // La cuenta se consulta en cada solicitud autenticada: desactivarla invalida sesiones JWT ya emitidas.
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async contexto =>
        {
            try
            {
                var idTexto = contexto.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idTexto, out var idUsuario))
                {
                    contexto.Fail("Token sin identificador de usuario válido.");
                    return;
                }

                var repositorio = contexto.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                var usuario = await repositorio.ObtenerPorIdAsync(idUsuario, contexto.HttpContext.RequestAborted);
                if (usuario is null || !usuario.Activo)
                    contexto.Fail("El usuario no está habilitado.");
            }
            catch
            {
                // Ante un fallo al consultar la cuenta no se permite continuar con una sesión no verificable.
                contexto.Fail("No fue posible validar el estado del usuario.");
            }
        }
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Orden del middleware: redirección HTTPS, CORS, identificación del usuario y permisos.
app.UseHttpsRedirection();
app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();

// Endpoint simple para confirmar que el servicio responde sin hacer login.
app.MapGet("/health", () => Results.Ok(new { estado = "correcto" }));

// Comprueba en modo solo lectura que PostgreSQL/Supabase acepta conexiones.
// No consulta ni modifica usuarios: sirve para confirmar que la API y la base están disponibles.
app.MapGet("/health/base-datos", async (AutoBrilloDbContext contexto, CancellationToken cancelacion) =>
{
    try
    {
        var conectada = await contexto.Database.CanConnectAsync(cancelacion);
        return conectada
            ? Results.Ok(new { estado = "correcto", baseDatos = "conectada" })
            : Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        // No se devuelven detalles internos ni cadenas de conexión al cliente.
        return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// Activa los endpoints definidos en los controladores, por ejemplo AuthController.
app.MapControllers();
app.Run();
