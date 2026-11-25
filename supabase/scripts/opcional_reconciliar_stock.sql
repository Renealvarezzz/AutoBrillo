-- OPCIONAL. No es una migración y nadie la ejecuta durante el despliegue.
-- Inserta un movimiento de ajuste por cada producto cuyo StockActual no coincide
-- con la suma de movimientos_stock. No cambia el saldo que ya ve el lavadero:
-- solo completa el historial para que ambos vuelvan a coincidir.
-- Es idempotente: si ya coinciden, no inserta filas.
-- Revisá el SELECT de abajo antes de ejecutar el INSERT. Hace falta al menos un usuario.

-- Vista previa (no escribe):
-- select p."Id_Productos", p."Descripcion", p."StockActual",
--        coalesce(sum(case when m."Tipo" = 'Entrada' then m."Cantidad" else -m."Cantidad" end), 0) as saldo_movimientos
-- from public.productos p
-- left join public.movimientos_stock m on m."Id_Productos" = p."Id_Productos"
-- group by p."Id_Productos", p."Descripcion", p."StockActual"
-- having p."StockActual" <> coalesce(sum(case when m."Tipo" = 'Entrada' then m."Cantidad" else -m."Cantidad" end), 0);

insert into public.movimientos_stock (
  "Id_Productos", "Fecha", "Tipo", "Origen", "Cantidad", "StockResultante", "Observacion", "Id_Usuarios"
)
select
  s."Id_Productos",
  now(),
  case when s.diferencia > 0 then 'Entrada' else 'Salida' end,
  'Ajuste',
  abs(s.diferencia),
  s."StockActual",
  'Ajuste opcional para reconciliar el saldo con el historial',
  (select u."Id_Usuarios" from public.usuarios u order by u."Id_Usuarios" limit 1)
from (
  select
    p."Id_Productos",
    p."StockActual",
    p."StockActual" - coalesce(sum(
      case when m."Tipo" = 'Entrada' then m."Cantidad" else -m."Cantidad" end
    ), 0) as diferencia
  from public.productos p
  left join public.movimientos_stock m on m."Id_Productos" = p."Id_Productos"
  group by p."Id_Productos", p."StockActual"
) s
where s.diferencia <> 0;
