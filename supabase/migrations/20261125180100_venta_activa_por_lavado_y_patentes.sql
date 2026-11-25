-- Una sola venta activa por lavado, y patentes comparables (sin espacios, en mayúsculas).
-- Idempotente. Aplicar junto con 20261125180000, antes de desplegar la API.
-- Si este índice no se puede crear, hay dos ventas no anuladas para el mismo lavado:
-- anular la duplicada y volver a ejecutar el archivo.

create unique index if not exists ux_ventas_lavado_activa
  on public.ventas ("Id_Lavados")
  where "Id_Lavados" is not null and "Estado" <> 'Anulada';

-- Normaliza chapas ya guardadas. Si dos patentes chocan al compactarse, se deja la original y se avisa.
do $$
declare
  fila record;
  nueva text;
begin
  for fila in
    select "Id_Vehiculos", "Patente"
    from public.vehiculos
    where "Patente" is distinct from upper(regexp_replace("Patente", '\s+', '', 'g'))
  loop
    nueva := upper(regexp_replace(fila."Patente", '\s+', '', 'g'));
    if exists (
      select 1 from public.vehiculos
      where "Id_Vehiculos" <> fila."Id_Vehiculos" and "Patente" = nueva
    ) then
      raise notice 'Patente % (id %) no se normalizó a % porque ya existe.', fila."Patente", fila."Id_Vehiculos", nueva;
    else
      update public.vehiculos set "Patente" = nueva where "Id_Vehiculos" = fila."Id_Vehiculos";
    end if;
  end loop;
end $$;
