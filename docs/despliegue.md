# Despliegue y configuración

## Infraestructura actual

| Componente | Plataforma | Función |
|---|---|---|
| Web Blazor WebAssembly | Vercel | Sirve los archivos estáticos de la interfaz. |
| API ASP.NET Core | VPS Hostinger | Ejecuta la API dentro de Docker. |
| Proxy HTTPS | Traefik | Dirige las solicitudes HTTPS al contenedor de la API. |
| Base de datos PostgreSQL | Supabase | Almacena los usuarios. |

## Web en Vercel

La web se publica como contenido estático. El archivo `src/AutoBrillo.Web/wwwroot/appsettings.json` contiene la dirección pública de la API:

```json
{
  "ApiUrl": "https://autobrillo-api.179.199.139.243.nip.io/"
}
```

Además, `wwwroot/vercel.json` configura el fallback SPA para que rutas como `/inicio` devuelvan `index.html` y Blazor pueda resolverlas en el navegador.

## API en Hostinger

El archivo `docker-compose.hostinger.yml` define el servicio de la API:

- Construye la imagen usando `Dockerfile.vercel`.
- Ejecuta el contenedor como `autobrillo-api`.
- Reinicia automáticamente mediante `restart: unless-stopped`.
- Expone el puerto solo en `127.0.0.1:8080`.
- Traefik recibe el tráfico HTTPS y lo dirige al puerto interno 8080.

El VPS utiliza un archivo local `.env.production` para la configuración. Ese archivo no se versiona.

## Variables necesarias en producción

| Variable | Uso |
|---|---|
| `CONNECTION_STRING` | Conexión de Entity Framework Core a PostgreSQL. |
| `JWT_SECRET` | Clave privada para firmar tokens JWT. |
| `JWT_ISSUER` | Emisor del token. Valor habitual: `AutoBrillo.Api`. |
| `JWT_AUDIENCE` | Audiencia del token. Valor habitual: `AutoBrillo.Web`. |
| `WEB_ORIGIN` | URL autorizada para CORS. |
| `PORT` | Puerto interno de la API. Docker usa `8080`. |

Ejemplo de estructura de `.env.production` **sin valores reales**:

```env
CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...;SSL Mode=Require
JWT_SECRET=clave-privada-larga
JWT_ISSUER=AutoBrillo.Api
JWT_AUDIENCE=AutoBrillo.Web
WEB_ORIGIN=https://autobrillo-web.vercel.app
PORT=8080
```

## Comandos de administración en el VPS

Estos comandos se ejecutan dentro de la carpeta donde está desplegada la API:

```bash
# Ver estado del contenedor
docker compose -f docker-compose.hostinger.yml ps

# Ver los últimos registros
docker compose -f docker-compose.hostinger.yml logs --tail 100 api

# Reconstruir y actualizar la API
docker compose -f docker-compose.hostinger.yml up -d --build

# Comprobar estado desde el servidor
curl http://127.0.0.1:8080/health
```

## Verificación posterior al despliegue

1. Abrir `https://autobrillo-web.vercel.app`.
2. Confirmar que aparece el formulario de inicio de sesión.
3. Iniciar sesión con un usuario existente.
4. Comprobar que se muestra la página Inicio.
5. Consultar `https://autobrillo-api.179.199.139.243.nip.io/health` y verificar la respuesta `{"estado":"correcto"}`.

## Buenas prácticas

- Nunca subir `.env.production`, tokens, contraseñas, hashes o cadenas de conexión a GitHub.
- Antes de publicar cambios, ejecutar `dotnet build AutoBrillo.slnx`.
- Mantener `WEB_ORIGIN` apuntando a la URL pública de Vercel para evitar accesos desde otros sitios web.
- Usar el endpoint `/health` para diagnosticar rápidamente si la API está disponible.
