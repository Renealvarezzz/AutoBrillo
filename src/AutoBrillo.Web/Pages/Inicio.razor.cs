namespace AutoBrillo.Web.Pages;

/// <summary>
/// Código de la página Inicio, separado de su vista Inicio.razor.
/// Su responsabilidad principal es evitar que se muestre el panel a quien no inició sesión.
/// </summary>
public partial class Inicio
{
    // Mientras se consulta la API, la vista muestra el mensaje "Comprobando tu sesión...".
    private bool cargando = true;

    /// <summary>
    /// Se ejecuta al abrir la página.
    /// Confirma con la API que el token guardado en el navegador es válido.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Si no existe sesión o el JWT es inválido, la persona vuelve al Login.
        if (!await Autenticacion.ValidarSesionAsync())
        {
            Navegacion.NavigateTo("login", forceLoad: true);
            return;
        }

        // Solo después de validar se muestra el contenido principal.
        cargando = false;
    }
}
