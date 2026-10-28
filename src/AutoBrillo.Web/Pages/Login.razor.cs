using Microsoft.AspNetCore.Components;

namespace AutoBrillo.Web.Pages;

/// <summary>
/// Código de la página de acceso, separado de su vista Login.razor.
/// La vista contiene el HTML; esta clase contiene el comportamiento del formulario.
/// </summary>
public partial class Login
{
    // Objeto que reúne los dos datos escritos en el formulario.
    private readonly Credenciales credenciales = new();

    // Referencia al input Usuario para colocar el cursor allí al abrir la pantalla.
    private ElementReference campoUsuario;

    // Evita que el usuario presione Ingresar varias veces mientras llega la respuesta.
    private bool enviando;

    // Evita mostrar el formulario mientras se confirma si ya existe una sesión válida.
    private bool cargandoSesion = true;

    // El foco se aplica una vez que el formulario ya fue renderizado.
    private bool enfocarCampoUsuario;

    /// <summary>
    /// Si la persona ya tiene una sesión válida, evita mostrar Login y la lleva al inicio.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (await Autenticacion.ValidarSesionAsync())
        {
            Navegacion.NavigateTo("inicio", forceLoad: true);
            return;
        }

        cargandoSesion = false;
        enfocarCampoUsuario = true;
    }

    /// <summary>
    /// Se ejecuta después de que Blazor dibuja la página por primera vez.
    /// Coloca el foco en Usuario para que se pueda escribir sin tocar el campo.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool primerRender)
    {
        if (enfocarCampoUsuario)
        {
            await campoUsuario.FocusAsync();
            enfocarCampoUsuario = false;
        }
    }

    /// <summary>
    /// Se ejecuta cuando el formulario es válido y se presiona Ingresar.
    /// Pide al servicio de autenticación iniciar sesión y redirige a Inicio si no hubo error.
    /// </summary>
    private async Task Ingresar()
    {
        enviando = true;
        var error = await Autenticacion.LoginAsync(credenciales.Nombre, credenciales.Password);
        enviando = false;

        // Null significa que LoginAsync recibió un token correcto de la API.
        if (error is null)
        {
            Navegacion.NavigateTo("inicio", forceLoad: true);
            return;
        }

        await Alertas.MostrarErrorGeneralAsync("No se pudo ingresar", error);
    }

    /// <summary>Modelo sencillo que representa los datos escritos en el formulario.</summary>
    private class Credenciales
    {
        /// <summary>Valor enlazado al campo Usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Valor enlazado al campo Contraseña; solo se envía a la API al ingresar.</summary>
        public string Password { get; set; } = string.Empty;
    }
}
