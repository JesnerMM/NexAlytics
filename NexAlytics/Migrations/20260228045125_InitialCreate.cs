using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NexAlytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DimClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DimFechas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Año = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    NombreMes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Trimestre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimFechas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DimProductos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DimProductos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StgClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportJobId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Procesado = table.Column<bool>(type: "bit", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StgClientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StgProductos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportJobId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Procesado = table.Column<bool>(type: "bit", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StgProductos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StgVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportJobId = table.Column<int>(type: "int", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Procesado = table.Column<bool>(type: "bit", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StgVentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FactPagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaId = table.Column<int>(type: "int", nullable: false),
                    PagoId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactPagos_DimFechas_FechaId",
                        column: x => x.FechaId,
                        principalTable: "DimFechas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    FechaId = table.Column<int>(type: "int", nullable: false),
                    ClienteDimId = table.Column<int>(type: "int", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactVentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactVentas_DimClientes_ClienteDimId",
                        column: x => x.ClienteDimId,
                        principalTable: "DimClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactVentas_DimFechas_FechaId",
                        column: x => x.FechaId,
                        principalTable: "DimFechas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchivoNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Registros = table.Column<int>(type: "int", nullable: false),
                    RegistrosProcesados = table.Column<int>(type: "int", nullable: false),
                    RegistrosError = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    Metodo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DimClientes",
                columns: new[] { "Id", "ClienteId", "EmpresaId", "Nombre" },
                values: new object[,]
                {
                    { 1, 1, 1, "Juan Pérez" },
                    { 2, 2, 1, "María García" },
                    { 3, 3, 1, "Carlos López" }
                });

            migrationBuilder.InsertData(
                table: "DimFechas",
                columns: new[] { "Id", "Año", "Fecha", "Mes", "NombreMes", "Trimestre" },
                values: new object[,]
                {
                    { 1, 2025, new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), 12, "Diciembre", 4 },
                    { 2, 2025, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 12, "Diciembre", 4 },
                    { 3, 2026, new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Enero", 1 },
                    { 4, 2026, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Enero", 1 }
                });

            migrationBuilder.InsertData(
                table: "DimProductos",
                columns: new[] { "Id", "Categoria", "EmpresaId", "Nombre", "ProductoId" },
                values: new object[,]
                {
                    { 1, "Electrónica", 1, "Laptop Pro", 1 },
                    { 2, "Periféricos", 1, "Mouse Inalámbrico", 2 },
                    { 3, "Electrónica", 1, "Monitor 27\"", 3 }
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Activo", "FechaCreacion", "Nombre" },
                values: new object[] { 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Empresa Demo S.A." });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Email", "EmpresaId", "FechaRegistro", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "juan@example.com", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Juan Pérez", "555-1001" },
                    { 2, "maria@example.com", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "María García", "555-1002" },
                    { 3, "carlos@example.com", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Carlos López", "555-1003" }
                });

            migrationBuilder.InsertData(
                table: "FactPagos",
                columns: new[] { "Id", "EmpresaId", "FechaId", "Monto", "PagoId" },
                values: new object[,]
                {
                    { 1, 1, 1, 1329.98m, 1 },
                    { 2, 1, 2, 399.99m, 2 },
                    { 3, 1, 4, 29.99m, 3 }
                });

            migrationBuilder.InsertData(
                table: "FactVentas",
                columns: new[] { "Id", "Cantidad", "ClienteDimId", "EmpresaId", "FechaId", "Total", "VentaId" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, 1, 1329.98m, 1 },
                    { 2, 1, 2, 1, 2, 399.99m, 2 },
                    { 3, 1, 3, 1, 3, 1299.99m, 3 },
                    { 4, 1, 1, 1, 4, 29.99m, 4 }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "Categoria", "EmpresaId", "Nombre", "Precio" },
                values: new object[,]
                {
                    { 1, true, "Electrónica", 1, "Laptop Pro", 1299.99m },
                    { 2, true, "Periféricos", 1, "Mouse Inalámbrico", 29.99m },
                    { 3, true, "Electrónica", 1, "Monitor 27\"", 399.99m }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "EmpresaId", "FechaCreacion", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { 1, "admin@demo.com", 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador", "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", "Admin" });

            migrationBuilder.InsertData(
                table: "Ventas",
                columns: new[] { "Id", "ClienteId", "EmpresaId", "Estado", "Fecha", "Total" },
                values: new object[,]
                {
                    { 1, 1, 1, "Completada", new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), 1329.98m },
                    { 2, 2, 1, "Completada", new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), 399.99m },
                    { 3, 3, 1, "Pendiente", new DateTime(2025, 12, 22, 0, 0, 0, 0, DateTimeKind.Utc), 1299.99m },
                    { 4, 1, 1, "Completada", new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), 29.99m }
                });

            migrationBuilder.InsertData(
                table: "Pagos",
                columns: new[] { "Id", "Fecha", "Metodo", "Monto", "VentaId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), "Tarjeta", 1329.98m, 1 },
                    { 2, new DateTime(2025, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Transferencia", 399.99m, 2 },
                    { 3, new DateTime(2025, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), "Efectivo", 29.99m, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId",
                table: "Clientes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FactPagos_FechaId",
                table: "FactPagos",
                column: "FechaId");

            migrationBuilder.CreateIndex(
                name: "IX_FactVentas_ClienteDimId",
                table: "FactVentas",
                column: "ClienteDimId");

            migrationBuilder.CreateIndex(
                name: "IX_FactVentas_FechaId",
                table: "FactVentas",
                column: "FechaId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_EmpresaId",
                table: "ImportJobs",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId",
                table: "Productos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EmpresaId",
                table: "Ventas",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DimProductos");

            migrationBuilder.DropTable(
                name: "FactPagos");

            migrationBuilder.DropTable(
                name: "FactVentas");

            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "StgClientes");

            migrationBuilder.DropTable(
                name: "StgProductos");

            migrationBuilder.DropTable(
                name: "StgVentas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "DimClientes");

            migrationBuilder.DropTable(
                name: "DimFechas");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
