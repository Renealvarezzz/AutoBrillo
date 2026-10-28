-- Convierte la relación inicial Usuario → Rol en Usuario ↔ Roles.
-- Conserva el rol ya asignado a cada usuario antes de retirar la columna antigua.
create table public.usuarios_roles (
  "Id_Usuarios" integer not null,
  "Id_Roles" integer not null,
  primary key ("Id_Usuarios", "Id_Roles"),
  constraint "FK_usuarios_roles_usuario" foreign key ("Id_Usuarios") references public.usuarios ("Id_Usuarios") on delete cascade,
  constraint "FK_usuarios_roles_rol" foreign key ("Id_Roles") references public.roles ("Id_Roles") on delete restrict
);

insert into public.usuarios_roles ("Id_Usuarios", "Id_Roles")
select "Id_Usuarios", "Id_Roles" from public.usuarios;

alter table public.usuarios drop constraint "FK_usuarios_roles_Id_Roles";
alter table public.usuarios drop column "Id_Roles";
alter table public.usuarios_roles enable row level security;
