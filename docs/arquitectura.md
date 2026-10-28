# Arquitectura de AutoBrillo

## Objetivo

AutoBrillo usa una arquitectura por capas para separar responsabilidades sin complicar el código. Cada proyecto tiene una función puntual y se comunica con el siguiente de manera ordenada.

```text
AutoBrillo.Web → API HTTP/JSON → Repository → Model → PostgreSQL
```

## Capas

### 1. AutoBrillo.Model

Contiene las entidades del sistema. No conoce la base de datos ni las pantallas.

Incluye entidades para `Usuario`, `Rol`, `Persona`, `TipoPersona`, `Producto` y `TipoProducto`.

Las relaciones N:M entre personas/tipos y productos/tipos se configuran mediante tablas puente en la capa Repository. Los campos y reglas del modelo administrativo se detallan en [Personas, clientes y productos](personas-y-productos.md).

### 2. AutoBrillo.Repository

Es la capa que se comunica con PostgreSQL usando Entity Framework Core.

Componentes principales:

- `Datos/AutoBrilloDbContext.cs`: configura usuarios, roles, personas, productos, catálogos de tipos y relaciones N:M.
- Repositorios e interfaces para usuarios, roles, personas, tipos de persona, productos y tipos de producto.
- `Seguridad/PasswordHasher.cs`: genera y verifica hashes BCrypt.

Esta separación permite reemplazar PostgreSQL por MariaDB en el futuro cambiando principalmente el proveedor de Entity Framework Core, sin modificar la web.

### 3. AutoBrillo.Api

Expone los datos y operaciones mediante HTTP/JSON.

Responsabilidades:

- Registro e inicio de sesión.
- Generación y validación de tokens JWT.
- Política CORS para permitir solo el origen de la web publicada.
- Endpoint de estado `/health`.
- Inyección de dependencias de Repository y DbContext.

La API no envía hashes de contraseñas al cliente.

### 4. AutoBrillo.Web

Es una aplicación Blazor WebAssembly. Se ejecuta en el navegador y usa `HttpClient` para comunicarse con la API.

Pantallas administrativas relevantes:

- `Login.razor`: formulario de acceso.
- `Inicio.razor`: pantalla inicial protegida.
- `Usuarios.razor` y `Roles.razor`: administración de accesos.
- `Personas.razor`: registro de clientes, funcionarios y proveedores.
- `Productos.razor`: registro de productos, servicios e insumos.
- `Shared/MainLayout.razor`: navegación y cierre de sesión.

Las pantallas administrativas validan la sesión y el rol `Administrador` antes de habilitar su contenido.

## Flujo de inicio de sesión

```mermaid
sequenceDiagram
    participant U as Usuario
    participant W as Web Blazor
    participant A as API
    participant R as Repository
    participant DB as PostgreSQL

    U->>W: Escribe nombre y contraseña
    W->>A: POST /api/auth/login
    A->>R: Buscar usuario por nombre
    R->>DB: Consulta tabla usuarios
    DB-->>R: Usuario y hash BCrypt
    R-->>A: Usuario encontrado
    A->>A: Verifica contraseña con BCrypt
    A-->>W: Token JWT y nombre
    W->>W: Guarda sesión en localStorage
    W-->>U: Muestra Inicio
```

## Decisiones de diseño

- Se eligieron nombres en español para facilitar la lectura y defensa en clase.
- No se utilizan microservicios, CQRS, MediatR ni capas adicionales.
- No existe proyecto de pruebas por alcance académico definido.
- La web no posee cadena de conexión ni acceso directo a Supabase.
- Las credenciales se suministran por variables de entorno, no desde el código fuente.
