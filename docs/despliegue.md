# Despliegue y configuración

Proyecto académico de la UPE (Paraguay). Esta página describe el entorno que está en producción.

## Infraestructura

| Componente | Dónde corre | Función |
|---|---|---|
| Web Blazor WebAssembly | Vercel, https://autobrillo-lemon.vercel.app | Archivos estáticos de la interfaz. AdminLTE 4.10.2 y DataTables 2.3.4 van dentro de `wwwroot/lib`. |
| API ASP.NET Core .NET 10 | VPS Hostinger, carpeta `/opt/autobrillo` | Contenedor `autobrillo-api`, servicio `api` de Docker Compose. |
| HTTPS | Caddy en el mismo VPS | Termina TLS de `api.autobrillo.tech` y reenvía a `127.0.0.1:8080`. |
| PostgreSQL | Supabase, región `sa-east-1` | 25 tablas en `public`, todas con RLS. La API entra por el session pooler. |
| Integración continua | GitHub Actions | `.github/workflows/build-test.yml` compila y prueba en pull requests y en cada push a `main`. |

Caddy no forma parte de `docker-compose.hostinger.yml`. Un ejemplo mínimo, marcado como ejemplo, está en [Caddyfile.ejemplo](Caddyfile.ejemplo).

## Web en Vercel

El archivo que Vercel lee es el `vercel.json` de la raíz del repositorio. Ahí están el comando de instalación de .NET 10, la publicación de `src/AutoBrillo.Web` y el directorio de salida `publicado/wwwroot`. El mismo archivo reescribe las rutas de Blazor (`/inicio`, `/lavados`, etc.) a `index.html`.

`dotnet publish` copia `wwwroot` al resultado. Por eso un `vercel.json` dentro de `wwwroot` terminaría publicado como archivo estático y no como configuración del proyecto: las opciones de build y las reescrituras ya están en la raíz. No hay una segunda copia en `src/AutoBrillo.Web/wwwroot`.

La dirección de la API está en `src/AutoBrillo.Web/wwwroot/appsettings.json`:

```json
{
  "ApiUrl": "https://api.autobrillo.tech/"
}
```

## API en el VPS

`docker-compose.hostinger.yml` define el servicio `api`:

- Imagen construida con `Dockerfile.api`.
- Contenedor `autobrillo-api`.
- `restart: unless-stopped`.
- Puerto publicado solo en `127.0.0.1:8080`.
- Zona horaria `America/Asuncion`.
- Variables en `.env.production`, que no se versiona.

El contenedor no abre el puerto 443. Caddy recibe HTTPS y hace de proxy inverso.

## Base de datos

La API usa el session pooler:

```text
aws-1-sa-east-1.pooler.supabase.com:5432
```

El host directo de Supabase es solo IPv6. Desde el VPS hay que usar el pooler.

Las migraciones están en `supabase/migrations` (19 archivos). Se pegan en el SQL Editor de Supabase, en el orden de la [lista de módulos](personas-y-productos.md), antes de desplegar una API que dependa de esas columnas. El historial de migraciones de Supabase queda vacío: no se aplican con la CLI ni con la integración de GitHub. Esa integración está desconectada a propósito.

## Variables

Nombres que lee `src/AutoBrillo.Api/Program.cs`. La plantilla sin secretos es [`.env.example`](../.env.example).

| Variable | Uso |
|---|---|
| `CONNECTION_STRING` | Cadena de Entity Framework Core. Usuario, contraseña y referencia del proyecto se reemplazan en el servidor. |
| `JWT_SECRET` | Clave para firmar los JWT. |
| `JWT_ISSUER` | Emisor. Valor habitual: `AutoBrillo.Api`. |
| `JWT_AUDIENCE` | Audiencia. Valor habitual: `AutoBrillo.Web`. |
| `WEB_ORIGIN` | Origen CORS. En producción: `https://autobrillo-lemon.vercel.app`. |
| `PORT` | Puerto interno. En Docker es `8080`. |

Ejemplo de forma, sin valores reales:

```env
CONNECTION_STRING=Host=aws-1-sa-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=REEMPLAZAR_USUARIO;Password=REEMPLAZAR_CONTRASEÑA;SSL Mode=Require
JWT_SECRET=REEMPLAZAR_CLAVE_LARGA_PRIVADA
JWT_ISSUER=AutoBrillo.Api
JWT_AUDIENCE=AutoBrillo.Web
WEB_ORIGIN=https://autobrillo-lemon.vercel.app
PORT=8080
```

## Volver a desplegar la API

Estos comandos se ejecutan en el VPS. El archivo `.env.production` vive solo en el servidor: el tarball de GitHub no lo trae, así que se recupera de la copia de respaldo.

```bash
cd /opt
BACKUP=/opt/autobrillo-backup-$(date +%Y%m%d%H%M)
cp -a /opt/autobrillo "$BACKUP"
curl -fsSL -o /tmp/autobrillo-main.tar.gz https://github.com/Renealvarezzz/AutoBrillo/archive/refs/heads/main.tar.gz
rm -rf /opt/autobrillo
mkdir -p /opt/autobrillo
tar -xzf /tmp/autobrillo-main.tar.gz --strip-components=1 -C /opt/autobrillo
cp "$BACKUP/.env.production" /opt/autobrillo/.env.production
cd /opt/autobrillo
docker compose -f docker-compose.hostinger.yml up -d --build
curl -fsS http://127.0.0.1:8080/health/base-datos
```

La respuesta esperada es `{"estado":"correcto","baseDatos":"conectada"}`. Desde afuera del VPS se puede repetir contra `https://api.autobrillo.tech/health/base-datos`.

Otros comandos útiles, dentro de `/opt/autobrillo`:

```bash
docker compose -f docker-compose.hostinger.yml ps
docker compose -f docker-compose.hostinger.yml logs --tail 100 api
curl -fsS http://127.0.0.1:8080/health
```

Un cambio de nombre de `Dockerfile.api` obliga a este redespliegue: Compose construye con el nombre que está en el archivo. El servicio, el contenedor, el puerto y las variables no cambian. Caddy tampoco se toca.

## Comprobación después de publicar

1. Abrir https://autobrillo-lemon.vercel.app y ver el formulario de acceso.
2. Entrar con un usuario activo.
3. Confirmar que el menú corresponde al rol.
4. Consultar `https://api.autobrillo.tech/health` y obtener `{"estado":"correcto"}`.
5. Consultar `https://api.autobrillo.tech/health/base-datos` y obtener la base conectada.

## Cuidados

- No subir `.env.production`, tokens, contraseñas ni cadenas con clave.
- Antes de publicar, `dotnet build AutoBrillo.slnx` y `dotnet test AutoBrillo.slnx`.
- `WEB_ORIGIN` debe ser la URL pública de Vercel, sin barra final.
- Aplicar en el SQL Editor cualquier migración nueva antes de levantar la API que la usa.
