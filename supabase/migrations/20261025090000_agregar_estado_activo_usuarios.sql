-- Permite habilitar o deshabilitar accesos sin eliminar usuarios.
-- Los registros existentes permanecen habilitados.
alter table public.usuarios
  add column if not exists "Activo" boolean not null default true;
