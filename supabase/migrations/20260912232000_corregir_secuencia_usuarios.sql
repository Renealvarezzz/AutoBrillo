-- Corrige la secuencia autoincremental de usuarios para que el próximo Id no repita uno existente.
select setval(
  pg_get_serial_sequence('public.usuarios', 'Id_Usuarios'),
  coalesce((select max("Id_Usuarios") from public.usuarios), 1),
  true
);
