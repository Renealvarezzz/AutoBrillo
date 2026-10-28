do $$
begin
  if exists (
    select 1
    from public.productos
    where "PrecioDeCosto" <> trunc("PrecioDeCosto")
       or "PrecioDeVenta" <> trunc("PrecioDeVenta")
       or "StockActual" <> trunc("StockActual")
  ) then
    raise exception 'No se puede aplicar productos_sin_decimales: existen precios o stock con decimales.';
  end if;
end $$;

alter table public.productos
  alter column "PrecioDeCosto" type numeric(18,0) using "PrecioDeCosto"::numeric(18,0),
  alter column "PrecioDeVenta" type numeric(18,0) using "PrecioDeVenta"::numeric(18,0),
  alter column "StockActual" type numeric(18,0) using "StockActual"::numeric(18,0);
