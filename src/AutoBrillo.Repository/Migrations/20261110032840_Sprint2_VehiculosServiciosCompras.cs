using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoBrillo.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_VehiculosServiciosCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Id_Personas = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Documento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.Id_Personas);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    Id_Productos = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PrecioDeCosto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioDeVenta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StockActual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.Id_Productos);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id_Roles = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id_Roles);
                });

            migrationBuilder.CreateTable(
                name: "tipos_personas",
                columns: table => new
                {
                    Id_Tipos_Personas = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_personas", x => x.Id_Tipos_Personas);
                });

            migrationBuilder.CreateTable(
                name: "tipos_productos",
                columns: table => new
                {
                    Id_Tipos_Productos = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_productos", x => x.Id_Tipos_Productos);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id_Usuarios = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id_Usuarios);
                });

            migrationBuilder.CreateTable(
                name: "personas_tipos",
                columns: table => new
                {
                    Id_Personas = table.Column<int>(type: "integer", nullable: false),
                    Id_Tipos_Personas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas_tipos", x => new { x.Id_Personas, x.Id_Tipos_Personas });
                    table.ForeignKey(
                        name: "FK_personas_tipos_personas_Id_Personas",
                        column: x => x.Id_Personas,
                        principalTable: "personas",
                        principalColumn: "Id_Personas",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_personas_tipos_tipos_personas_Id_Tipos_Personas",
                        column: x => x.Id_Tipos_Personas,
                        principalTable: "tipos_personas",
                        principalColumn: "Id_Tipos_Personas",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "productos_tipos",
                columns: table => new
                {
                    Id_Productos = table.Column<int>(type: "integer", nullable: false),
                    Id_Tipos_Productos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos_tipos", x => new { x.Id_Productos, x.Id_Tipos_Productos });
                    table.ForeignKey(
                        name: "FK_productos_tipos_productos_Id_Productos",
                        column: x => x.Id_Productos,
                        principalTable: "productos",
                        principalColumn: "Id_Productos",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_productos_tipos_tipos_productos_Id_Tipos_Productos",
                        column: x => x.Id_Tipos_Productos,
                        principalTable: "tipos_productos",
                        principalColumn: "Id_Tipos_Productos",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_roles",
                columns: table => new
                {
                    Id_Usuarios = table.Column<int>(type: "integer", nullable: false),
                    Id_Roles = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_roles", x => new { x.Id_Usuarios, x.Id_Roles });
                    table.ForeignKey(
                        name: "FK_usuarios_roles_roles_Id_Roles",
                        column: x => x.Id_Roles,
                        principalTable: "roles",
                        principalColumn: "Id_Roles",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_usuarios_Id_Usuarios",
                        column: x => x.Id_Usuarios,
                        principalTable: "usuarios",
                        principalColumn: "Id_Usuarios",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personas_Documento",
                table: "personas",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personas_tipos_Id_Tipos_Personas",
                table: "personas_tipos",
                column: "Id_Tipos_Personas");

            migrationBuilder.CreateIndex(
                name: "IX_productos_Descripcion",
                table: "productos",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_tipos_Id_Tipos_Productos",
                table: "productos_tipos",
                column: "Id_Tipos_Productos");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Descripcion",
                table: "roles",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_personas_Descripcion",
                table: "tipos_personas",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_productos_Descripcion",
                table: "tipos_productos",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Nombre",
                table: "usuarios",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roles_Id_Roles",
                table: "usuarios_roles",
                column: "Id_Roles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personas_tipos");

            migrationBuilder.DropTable(
                name: "productos_tipos");

            migrationBuilder.DropTable(
                name: "usuarios_roles");

            migrationBuilder.DropTable(
                name: "personas");

            migrationBuilder.DropTable(
                name: "tipos_personas");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "tipos_productos");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
