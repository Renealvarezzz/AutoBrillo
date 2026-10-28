-- AutoBrillo accede a estas tablas exclusivamente mediante la API.
-- RLS impide que los roles anónimos o autenticados de Supabase las consulten
-- directamente con la URL y una clave pública del proyecto.
alter table public.personas enable row level security;
alter table public.personas_tipos enable row level security;
alter table public.productos enable row level security;
alter table public.productos_tipos enable row level security;
alter table public.tipos_personas enable row level security;
alter table public.tipos_productos enable row level security;
