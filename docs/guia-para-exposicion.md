# Guía para entender y explicar el código

Esta guía está pensada para que el grupo pueda explicar AutoBrillo aun teniendo poca experiencia en programación. No hace falta memorizar cada línea: lo importante es entender el recorrido de los datos.

## La explicación corta para el profesor

> “AutoBrillo está separado en cuatro capas. La Web muestra las pantallas y llama a la API. La API recibe las solicitudes, valida el login y pide datos a Repository. Repository es la capa que usa Entity Framework Core para hablar con PostgreSQL. Model representa los datos, por ejemplo el usuario. De esta forma cada capa tiene una sola responsabilidad y el código es más fácil de mantener.”

## 1. Cómo leer la solución

| Capa | Pregunta que responde | Archivo principal para mostrar |
|---|---|---|
| `AutoBrillo.Model` | ¿Qué datos tiene el sistema? | `Entidades/Usuario.cs` |
| `AutoBrillo.Repository` | ¿Cómo se guarda o busca un usuario? | `Repositorios/RepositorioUsuarios.cs` |
| `AutoBrillo.Api` | ¿Qué solicitud recibe la aplicación? | `Controllers/AuthController.cs` |
| `AutoBrillo.Web` | ¿Qué ve y usa la persona? | `Pages/Login.razor` |

## 2. Ejemplo completo: iniciar sesión

Este es el mejor flujo para explicar en una exposición.

### Paso 1: La persona escribe sus datos en la Web

En `AutoBrillo.Web/Pages/Login.razor` están los campos:

```razor
<label>Usuario <input @bind="credenciales.Nombre" /></label>
<label>Contraseña <input @bind="credenciales.Password" type="password" /></label>
```

- `@bind` conecta el input visual con una variable de C#.
- Cuando se escribe `admin`, el valor pasa a `credenciales.Nombre`.
- Al presionar **Ingresar**, se ejecuta `Ingresar()` de `Login.razor.cs`.

### Paso 2: La Web llama a la API

`Login.razor.cs` usa `ServicioAutenticacion`:

```csharp
error = await Autenticacion.LoginAsync(credenciales.Nombre, credenciales.Password);
```

Y el servicio envía JSON a la API:

```csharp
var respuesta = await http.PostAsJsonAsync(
    "api/auth/login",
    new { nombre, password });
```

Esto significa: “enviar una solicitud POST al endpoint `/api/auth/login` con nombre y contraseña”. La Web no consulta la base de datos por sí misma.

### Paso 3: La API recibe el login

En `AutoBrillo.Api/Controllers/AuthController.cs` está el endpoint:

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(CredencialesRequest request, CancellationToken cancellationToken)
```

- `[HttpPost("login")]` convierte el método en la ruta `POST /api/auth/login`.
- `request` contiene `Nombre` y `Password` recibidos como JSON.
- `IActionResult` permite devolver respuestas HTTP: `200`, `401`, `409`, etc.

### Paso 4: Repository busca el usuario

La API no hace SQL directamente. Pide a Repository buscar el usuario:

```csharp
var usuario = await usuarios.ObtenerPorNombreAsync(request.Nombre.Trim(), cancellationToken);
```

La implementación está en `RepositorioUsuarios.cs`:

```csharp
contexto.Usuarios.SingleOrDefaultAsync(x => x.Nombre == nombre, cancellationToken);
```

En palabras simples: “en la tabla `usuarios`, buscar uno cuyo `Nombre` sea igual al recibido”. Si no hay uno, devuelve `null`.

### Paso 5: BCrypt verifica la contraseña

```csharp
passwordHasher.Verificar(request.Password, usuario.Password)
```

- `request.Password`: lo que escribió la persona.
- `usuario.Password`: el hash BCrypt guardado en PostgreSQL.
- `Verificar` responde `true` cuando ambos coinciden.

La contraseña original no queda guardada en la base de datos.

### Paso 6: La API devuelve un token

Cuando las credenciales son correctas, la API crea un JWT:

```csharp
return Ok(new { token = CrearToken(usuario), usuario = usuario.Nombre });
```

El token es una prueba temporal de que el usuario inició sesión. La Web lo guarda en el navegador para enviarlo al abrir la página Inicio.

### Paso 7: La página Inicio valida la sesión

`Inicio.razor.cs` llama a `ValidarSesionAsync()`. Ese método envía el token en el encabezado HTTP:

```csharp
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
```

La API valida ese token antes de responder a `GET /api/auth/me`. Si es válido, se muestra el panel; si no, la Web vuelve a Login.

## 3. Ejemplo: registrar un usuario

La API registra con `POST /api/auth/register`.

```csharp
var usuario = new Usuario
{
    Nombre = nombre,
    Password = passwordHasher.Hashear(request.Password)
};

await usuarios.AgregarAsync(usuario, cancellationToken);
await usuarios.GuardarCambiosAsync(cancellationToken);
```

Forma fácil de explicarlo:

1. Se limpia el nombre con `Trim()`.
2. Se comprueba que no esté repetido.
3. BCrypt transforma la contraseña en un hash.
4. Repository prepara el usuario.
5. `GuardarCambiosAsync` ejecuta el `INSERT` en PostgreSQL.

## 4. Qué significa cada término frecuente

| Término | Explicación simple |
|---|---|
| Clase | Molde que define datos y acciones. `Usuario` es una clase. |
| Objeto | Una instancia concreta de una clase. Ejemplo: el usuario `admin`. |
| Método | Acción dentro de una clase. Ejemplo: `LoginAsync`. |
| `async` / `await` | Permiten esperar una operación lenta, como una consulta web o de base, sin bloquear la pantalla. |
| API | Puente HTTP que permite a la Web pedir datos de forma controlada. |
| JSON | Formato de texto usado para intercambiar datos entre Web y API. |
| Repository | Capa que concentra el acceso a datos. |
| Entity Framework Core | Herramienta que permite usar clases C# para trabajar con tablas. |
| JWT | Token temporal que representa una sesión iniciada. |
| BCrypt | Algoritmo que transforma una contraseña en hash no reversible. |
| Inyección de dependencias | ASP.NET crea y entrega objetos necesarios, como Repository, al controlador. |
| CORS | Regla que decide qué sitio web puede llamar a la API desde un navegador. |

## 5. Guion recomendado para la demostración

1. Mostrar el diagrama de [arquitectura](arquitectura.md).
2. Abrir `Usuario.cs` y explicar que Model solo representa los datos.
3. Abrir `RepositorioUsuarios.cs` y explicar que allí se centralizan consultas a PostgreSQL.
4. Abrir `AuthController.cs` y mostrar el método `Login`.
5. Abrir `Login.razor` y mostrar los inputs y el botón.
6. Ejecutar login en la aplicación.
7. Abrir `Inicio.razor.cs` y explicar que valida la sesión antes de mostrar el panel.

## 6. Preguntas probables y respuestas breves

**¿Por qué usar capas?**

Para separar responsabilidades. Así la interfaz no conoce SQL y la base de datos no depende de la pantalla.

**¿Por qué la Web no se conecta directamente a Supabase?**

Porque la API controla las operaciones y protege la cadena de conexión. La Web solo usa HTTP/JSON.

**¿Por qué usar Repository?**

Para concentrar el acceso a datos en un solo lugar. Si se cambia PostgreSQL por MariaDB, el impacto queda principalmente en esa capa.

**¿Por qué BCrypt?**

Para no guardar la contraseña original. BCrypt guarda un hash y luego permite comprobar si la contraseña ingresada coincide.

**¿Para qué sirve JWT?**

Para mantener una sesión temporal después del login sin enviar la contraseña en cada solicitud.

**¿Qué hace CORS?**

Permite que la Web publicada en Vercel pueda llamar a la API y bloquea otros orígenes no autorizados.

## 7. Comentarios dentro del código

Los archivos principales tienen comentarios de dos tipos:

- `// comentario`: explica una línea o bloque puntual.
- `/// <summary>`: explica una clase, propiedad o método. Visual Studio puede mostrarlo como ayuda al pasar el cursor.

La recomendación es leer primero los comentarios y luego la línea que explican. No es necesario repetirlos literalmente en la exposición: sirven para entender la intención de cada parte.
