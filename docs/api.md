# Referencia de la API

Dirección publicada: `https://api.autobrillo.tech/`. Las respuestas son JSON. Los nombres de propiedades salen en camelCase (`id_Usuarios`, `esAdministrador`).

Salvo `/health`, `/health/base-datos` y `POST /api/auth/login`, hace falta:

```http
Authorization: Bearer TOKEN_JWT
```

El token dura 8 horas. Trae el identificador, el nombre y los roles. Si el usuario pasa a inactivo, el token deja de aceptarse.

Una `ReglaNegocioException` se convierte en `400` con `{ "mensaje": "..." }`. No se documentan aquí todas las frases: salen del código de la regla que falló.

Las tablas y el orden de las migraciones están en [Módulos, tablas y migraciones](personas-y-productos.md).

## Roles

| Rol | Puede |
|---|---|
| `Administrador` | Todas las rutas, incluidas altas, anulaciones, precios y comisiones. |
| `Supervisor`, `Caja` | Leer personas, productos y vehículos. Registrar, editar y finalizar lavados. Registrar ventas y cobros. |
| `Stock` | Leer personas y productos. Registrar compras y movimientos de stock. |

Anular una compra, un lavado o una venta, y cambiar precios o comisiones, queda en `Administrador`.

## Estado

### `GET /health`

Público. La API responde sin abrir la base.

```json
{ "estado": "correcto" }
```

### `GET /health/base-datos`

Público y de solo lectura. Comprueba que PostgreSQL acepta la conexión. No devuelve la cadena ni datos de usuarios.

```json
{ "estado": "correcto", "baseDatos": "conectada" }
```

Si la base no responde, el estado HTTP es `503`.

## Autenticación — `/api/auth`

### `POST /api/auth/login`

Público. Body:

```json
{ "nombre": "ana", "password": "clave-de-al-menos-6" }
```

`200`:

```json
{
  "token": "jwt",
  "usuario": "ana",
  "roles": ["Administrador"],
  "esAdministrador": true
}
```

`401` si el nombre no existe, la contraseña no coincide o el usuario está inactivo. El mensaje no dice cuál de los tres falló: `Nombre o contraseña incorrectos.`

### `POST /api/auth/register`

Solo `Administrador`. Crea el usuario, le asigna el rol `Administrador` y guarda la contraseña como hash BCrypt.

Mismo body que el login. `201`:

```json
{ "id_Usuarios": 2, "nombre": "ana" }
```

- `400`: faltan datos o la contraseña tiene menos de 6 caracteres.
- `401` o `403`: no hay token o el rol no es `Administrador`.
- `409`: el nombre ya existe.

El ABM de usuarios (`/api/usuarios`) permite elegir otros roles. Este endpoint no.

### `GET /api/auth/me`

Cualquier usuario autenticado.

```json
{
  "id": "1",
  "usuario": "ana",
  "esAdministrador": true,
  "roles": ["Administrador"]
}
```

`401` si el token falta, venció o el usuario está inactivo.

## Usuarios y roles

Todo el grupo exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/usuarios` | Lista usuarios, roles, estado y persona vinculada. Nunca devuelve el hash. |
| `POST` | `/api/usuarios` | Crea. Contraseña obligatoria, mínimo 6 caracteres, al menos un rol. |
| `PUT` | `/api/usuarios/{id}` | Edita nombre, roles, persona y, si viene, la contraseña. |
| `PATCH` | `/api/usuarios/{id}/estado` | Body `{ "activo": true }`. No se puede desactivar la propia cuenta. |
| `DELETE` | `/api/usuarios/{id}` | No se puede borrar la propia cuenta. |
| `GET` | `/api/roles` | Lista roles. |
| `POST` | `/api/roles` | Crea un rol. |
| `PUT` | `/api/roles/{id}` | Cambia la descripción. |
| `DELETE` | `/api/roles/{id}` | Rechaza el rol si tiene usuarios. |

Siempre debe quedar al menos un administrador activo. Una persona no puede tener dos cuentas.

Ejemplo de alta:

```json
{
  "nombre": "caja1",
  "password": "clave-de-al-menos-6",
  "id_Roles": [3],
  "id_Personas": 10
}
```

## Personas y tipos

`GET /api/personas` acepta `Administrador`, `Supervisor`, `Caja` y `Stock`. El resto de esta tabla exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/personas` | Lista personas y sus tipos. |
| `POST` | `/api/personas` | Crea. Nombre, documento y al menos un tipo. |
| `PUT` | `/api/personas/{id}` | Edita. |
| `PATCH` | `/api/personas/{id}/estado` | Activa o desactiva. |
| `DELETE` | `/api/personas/{id}` | Elimina. |
| `GET` | `/api/tipos-personas` | Lista tipos. |
| `POST` | `/api/tipos-personas` | Crea un tipo. |
| `PUT` | `/api/tipos-personas/{id}` | Edita. |
| `DELETE` | `/api/tipos-personas/{id}` | Solo si ninguna persona lo usa. |

```json
{
  "nombre": "Ana Gómez",
  "documento": "1234567",
  "telefono": "0981000000",
  "id_Tipos_Personas": [1]
}
```

Los tipos iniciales son `Cliente`, `Funcionario` y `Proveedor`. En la interfaz, un cliente es una persona que tiene ese tipo.

## Productos y tipos

Misma división de lectura y escritura que personas.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/productos` | Lista productos, precios, stock y tipos. |
| `POST` | `/api/productos` | Crea. El stock inicial genera el movimiento correspondiente. |
| `PUT` | `/api/productos/{id}` | Edita descripción, precios y tipos. El stock no se corrige por esta ruta. |
| `PATCH` | `/api/productos/{id}/estado` | Activa o desactiva. |
| `DELETE` | `/api/productos/{id}` | Elimina. |
| `GET` `POST` | `/api/tipos-productos` | Lista o crea tipos. |
| `PUT` `DELETE` | `/api/tipos-productos/{id}` | Edita o elimina un tipo sin productos. |

Precios y stock son enteros en guaraníes, no negativos. Los tipos iniciales son `Servicio`, `Producto`, `Insumo` y `Adicional`.

```json
{
  "descripcion": "Lavado completo",
  "precioDeCosto": 25000,
  "precioDeVenta": 50000,
  "stockActual": 0,
  "id_Tipos_Productos": [1]
}
```

## Vehículos y tipos de vehículo

La lectura (`GET`) acepta `Administrador`, `Supervisor` y `Caja`. Crear, editar y cambiar estado exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/vehiculos?idPersona=` | Lista. `idPersona` es opcional. |
| `POST` | `/api/vehiculos` | Crea. |
| `PUT` | `/api/vehiculos/{id}` | Edita. |
| `PATCH` | `/api/vehiculos/{id}/estado` | Activa o desactiva. |
| `GET` | `/api/tipos-vehiculos` | Lista tipos. |
| `POST` | `/api/tipos-vehiculos` | Crea. Body `{ "descripcion": "Auto", "activo": true }`. |
| `PUT` | `/api/tipos-vehiculos/{id}` | Edita. |

Tipos iniciales: `Moto`, `Auto`, `Camioneta`, `Furgón / Utilitario`. La patente se guarda en mayúsculas y sin espacios.

## Servicios, precios e insumos

Todo este grupo exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/precios-servicios` | Precios con `Vigente` en true, por producto y tipo de vehículo. |
| `PUT` | `/api/precios-servicios` | Deja el precio anterior como no vigente y guarda una fila nueva. Si el importe no cambió, solo actualiza fecha y usuario. |
| `DELETE` | `/api/precios-servicios/{id}` | Marca esa fila como no vigente. No borra el historial. |
| `GET` | `/api/servicios/precio?idServicio=&idTipoVehiculo=&adicionales=` | Calcula precio base, adicionales y total. |
| `GET` | `/api/servicios/insumos?idServicio=` | Insumos configurados para un servicio. |
| `PUT` | `/api/servicios/insumos` | Asocia un insumo y una cantidad. |
| `DELETE` | `/api/servicios/insumos/{id}` | Quita la asociación. |
| `POST` | `/api/servicios/{id}/consumo` | Descuenta del stock los insumos de ese servicio. |

Body de un precio:

```json
{ "id_Productos": 1, "id_Tipos_Vehiculos": 2, "precio": 50000 }
```

El historial queda en `precios_servicios`. La consulta de listado solo devuelve la fila vigente. Hay como máximo una fila vigente por producto y tipo de vehículo.

## Inventario

`Administrador` o `Stock`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/movimientos-stock?idProducto=` | Movimientos. El filtro de producto es opcional. |
| `POST` | `/api/movimientos-stock` | Registra entrada o salida y devuelve el stock resultante. |

```json
{
  "id_Productos": 4,
  "tipo": "Entrada",
  "origen": "Ajuste",
  "cantidad": 10,
  "observacion": "Conteo de depósito"
}
```

## Compras

`Administrador` o `Stock` para listar y registrar. Anular exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/compras` | Compras con proveedor, totales y detalle. |
| `POST` | `/api/compras` | Registra la compra y suma stock. |
| `POST` | `/api/compras/{id}/anular` | Anula. Body `{ "motivo": "Error de carga" }`. |

```json
{
  "id_Personas": 8,
  "comprobante": "001-001-0000001",
  "detalles": [
    { "id_Productos": 4, "cantidad": 10, "precioUnitario": 8000 }
  ]
}
```

## Lavados

`Administrador`, `Supervisor` o `Caja`. Cancelar exige `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/lavados` | Lavados con vehículo, servicios, participantes e historial de estados. |
| `POST` | `/api/lavados` | Registra. |
| `PUT` | `/api/lavados/{id}` | Edita uno que siga editable. |
| `POST` | `/api/lavados/{id}/finalizar` | Cierra el lavado. |
| `POST` | `/api/lavados/{id}/cancelar` | Anula. Body `{ "motivo": "..." }`. |

```json
{
  "id_Vehiculos": 3,
  "sector": 1,
  "fecha": "2026-11-25T09:00:00",
  "id_Servicios": [1],
  "id_Adicionales": [6],
  "id_Participantes": [10, 11]
}
```

## Ventas y cobros

Listar, registrar y cobrar: `Administrador`, `Supervisor` o `Caja`. Anular: `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/ventas` | Ventas, detalle, cobros y total cobrado. |
| `POST` | `/api/ventas` | Registra. Puede vincular un lavado. |
| `POST` | `/api/ventas/{id}/cobros` | Registra un cobro. |
| `POST` | `/api/ventas/{id}/anular` | Anula. Body `{ "motivo": "..." }`. |

Hay como máximo una venta no anulada por lavado.

```json
{
  "id_Lavados": 5,
  "condicion": "Contado",
  "detalles": [{ "id_Productos": 1, "cantidad": 1 }]
}
```

Cobro:

```json
{ "medioPago": "Efectivo", "importe": 50000 }
```

## Comisiones

Solo `Administrador`.

| Método | Ruta | Acción |
|---|---|---|
| `GET` | `/api/comisiones` | Porcentaje vigente e historial de comisiones. |
| `PUT` | `/api/comisiones/porcentaje` | Body `{ "porcentaje": 30 }`. Aplica a las ventas nuevas. |

## Ejemplos con cURL

Estado de la API y de la base:

```bash
curl -fsS https://api.autobrillo.tech/health
curl -fsS https://api.autobrillo.tech/health/base-datos
```

Login:

```bash
curl -sS -X POST https://api.autobrillo.tech/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"nombre":"USUARIO","password":"CONTRASEÑA"}'
```

Registro, con el token de un administrador. No funciona sin ese encabezado:

```bash
curl -sS -X POST https://api.autobrillo.tech/api/auth/register \
  -H 'Content-Type: application/json' \
  -H 'Authorization: Bearer TOKEN' \
  -d '{"nombre":"nuevo","password":"clave-de-al-menos-6"}'
```

No incluir contraseñas reales, tokens ni cadenas de conexión en capturas ni en el repositorio.
