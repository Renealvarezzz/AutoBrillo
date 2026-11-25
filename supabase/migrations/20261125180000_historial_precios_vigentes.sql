-- Historial de precios por servicio y tipo de vehículo.
-- La fila vigente es la que usan los lavados. Las anteriores quedan en la misma tabla.
-- Idempotente. Aplicar ANTES de desplegar la API que lee la columna "Vigente".

alter table public.precios_servicios
  add column if not exists "Vigente" boolean not null default true;

alter table public.precios_servicios
  drop constraint if exists precios_servicios_unico;

create unique index if not exists ux_precios_servicios_vigente
  on public.precios_servicios ("Id_Productos", "Id_Tipos_Vehiculos")
  where "Vigente" = true;
