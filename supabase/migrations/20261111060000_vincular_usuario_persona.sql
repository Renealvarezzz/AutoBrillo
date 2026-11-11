-- PBI-004: una persona puede tener una cuenta de usuario, y la cuenta es opcional.
-- La columna es nullable para no tocar las cuentas que ya existen.
alter table public.usuarios
  add column if not exists "Id_Personas" integer null references public.personas("Id_Personas") on delete set null;

create unique index if not exists ux_usuarios_persona
  on public.usuarios ("Id_Personas")
  where "Id_Personas" is not null;
