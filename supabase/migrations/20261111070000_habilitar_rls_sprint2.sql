-- Igual que el resto de las tablas: la web no consulta Supabase directo.
-- RLS sin políticas públicas deja el acceso solo a la conexión de la API.
alter table public.tipos_vehiculos enable row level security;
alter table public.precios_servicios enable row level security;
alter table public.vehiculos enable row level security;
alter table public.movimientos_stock enable row level security;
alter table public.compras enable row level security;
alter table public.compras_detalle enable row level security;
alter table public.servicios_insumos enable row level security;
