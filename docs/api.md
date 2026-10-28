# Referencia de la API

## Dirección publicada

```text
https://autobrillo-api.179.199.139.243.nip.io/
```

Todas las respuestas usan JSON.

> Los endpoints administrativos de Personas, Productos y sus tipos están documentados junto con sus tablas y campos en [Personas, clientes y productos](personas-y-productos.md).

## Estado del servicio

### `GET /health`

Sirve para comprobar que la API está disponible.

**Respuesta exitosa — 200**

```json
{
  "estado": "correcto"
}
```

## Autenticación

### `POST /api/auth/register`

Registra un nuevo usuario. La contraseña se convierte a hash BCrypt dentro de `AutoBrillo.Repository` antes de guardarse.

**Cuerpo de la solicitud**

```json
{
  "nombre": "nuevo_usuario",
  "password": "una-contraseña-de-al-menos-6-caracteres"
}
```

**Respuesta exitosa — 201**

```json
{
  "id_Usuarios": 1,
  "nombre": "nuevo_usuario"
}
```

**Posibles respuestas**

- `400 Bad Request`: faltan datos o la contraseña tiene menos de 6 caracteres.
- `409 Conflict`: el nombre de usuario ya existe.

### `POST /api/auth/login`

Comprueba las credenciales y devuelve un token JWT válido por 8 horas.

**Cuerpo de la solicitud**

```json
{
  "nombre": "nuevo_usuario",
  "password": "una-contraseña-de-al-menos-6-caracteres"
}
```

**Respuesta exitosa — 200**

```json
{
  "token": "token-jwt-generado-por-la-api",
  "usuario": "nuevo_usuario"
}
```

**Respuesta de error — 401**

```json
{
  "mensaje": "Nombre o contraseña incorrectos."
}
```

### `GET /api/auth/me`

Devuelve los datos básicos del usuario autenticado.

**Encabezado requerido**

```http
Authorization: Bearer TOKEN_JWT
```

**Respuesta exitosa — 200**

```json
{
  "id": "1",
  "usuario": "nuevo_usuario"
}
```

**Posibles respuestas**

- `401 Unauthorized`: falta el token, venció o no es válido.

## Ejemplos con cURL

Comprobar el estado:

```bash
curl https://autobrillo-api.179.199.139.243.nip.io/health
```

Registrar un usuario:

```bash
curl -X POST https://autobrillo-api.179.199.139.243.nip.io/api/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"nombre":"ejemplo","password":"clave-segura"}'
```

Iniciar sesión:

```bash
curl -X POST https://autobrillo-api.179.199.139.243.nip.io/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"nombre":"ejemplo","password":"clave-segura"}'
```

> No se deben incluir contraseñas reales, tokens JWT ni cadenas de conexión en capturas, documentación ni commits.
