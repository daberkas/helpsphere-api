using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API_TFCAppDavid.Migrations
{
    /// <inheritdoc />
    public partial class InitialHelpSphere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.IdCategoria);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirebaseUid = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotoPerfil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zona = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaldoPuntos = table.Column<int>(type: "int", nullable: false),
                    ReputacionMedia = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publicaciones",
                columns: table => new
                {
                    IdPublicacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoPublicacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zona = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PuntosEstimados = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaServicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdUsuarioCreador = table.Column<int>(type: "int", nullable: false),
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicaciones", x => x.IdPublicacion);
                    table.ForeignKey(
                        name: "FK_Publicaciones_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publicaciones_Usuarios_IdUsuarioCreador",
                        column: x => x.IdUsuarioCreador,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosPuntos",
                columns: table => new
                {
                    IdMovimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdPublicacion = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosPuntos", x => x.IdMovimiento);
                    table.ForeignKey(
                        name: "FK_MovimientosPuntos_Publicaciones_IdPublicacion",
                        column: x => x.IdPublicacion,
                        principalTable: "Publicaciones",
                        principalColumn: "IdPublicacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosPuntos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesParticipacion",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdPublicacion = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioSolicitante = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesParticipacion", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesParticipacion_Publicaciones_IdPublicacion",
                        column: x => x.IdPublicacion,
                        principalTable: "Publicaciones",
                        principalColumn: "IdPublicacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesParticipacion_Usuarios_IdUsuarioSolicitante",
                        column: x => x.IdUsuarioSolicitante,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Valoraciones",
                columns: table => new
                {
                    IdValoracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Puntuacion = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaValoracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdPublicacion = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioEmisor = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioReceptor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valoraciones", x => x.IdValoracion);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Publicaciones_IdPublicacion",
                        column: x => x.IdPublicacion,
                        principalTable: "Publicaciones",
                        principalColumn: "IdPublicacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Usuarios_IdUsuarioEmisor",
                        column: x => x.IdUsuarioEmisor,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Usuarios_IdUsuarioReceptor",
                        column: x => x.IdUsuarioReceptor,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "IdCategoria", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ayuda, acompañamiento y recados para personas mayores", "Personas mayores" },
                    { 2, "Paseo y cuidado puntual de animales domésticos", "Mascotas" },
                    { 3, "Ayuda con móviles, ordenadores y programas", "Informática" },
                    { 4, "Apoyo educativo o formación básica", "Clases" },
                    { 5, "Compras, gestiones y pequeñas tareas cotidianas", "Recados" },
                    { 6, "Pequeñas reparaciones domésticas", "Reparaciones" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "IdRol", "NombreRol" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Usuario" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "IdUsuario", "Activo", "Apellidos", "Descripcion", "Email", "FechaRegistro", "FirebaseUid", "FotoPerfil", "IdRol", "Nombre", "ReputacionMedia", "SaldoPuntos", "Telefono", "Zona" },
                values: new object[,]
                {
                    { 1, true, "Bermudez", "Usuario administrador inicial para pruebas.", "admin@helpsphere.local", new DateTime(2026, 5, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), "firebase-admin-demo-001", null, 1, "David", 5.00m, 100, "600111222", "Cordoba" },
                    { 2, true, "García", "Usuaria de prueba interesada en ofrecer ayuda.", "laura@helpsphere.local", new DateTime(2026, 5, 3, 10, 15, 0, 0, DateTimeKind.Unspecified), "firebase-user-demo-002", null, 2, "Laura", 4.50m, 50, "600222333", "Cordoba" },
                    { 3, true, "Sánchez", "Usuario de prueba que solicita ayuda puntual.", "miguel@helpsphere.local", new DateTime(2026, 5, 3, 10, 30, 0, 0, DateTimeKind.Unspecified), "firebase-user-demo-003", null, 2, "Miguel", 0.00m, 30, "600333444", "Cordoba" }
                });

            migrationBuilder.InsertData(
                table: "MovimientosPuntos",
                columns: new[] { "IdMovimiento", "Cantidad", "Descripcion", "FechaMovimiento", "IdPublicacion", "IdUsuario", "TipoMovimiento" },
                values: new object[] { 1, 100, "Saldo inicial de prueba para administrador.", new DateTime(2026, 5, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), null, 1, "AJUSTE" });

            migrationBuilder.InsertData(
                table: "Publicaciones",
                columns: new[] { "IdPublicacion", "Descripcion", "Estado", "FechaCreacion", "FechaServicio", "IdCategoria", "IdUsuarioCreador", "PuntosEstimados", "TipoPublicacion", "Titulo", "Zona" },
                values: new object[,]
                {
                    { 1, "Necesito ayuda para instalar programas básicos y configurar el correo electrónico.", "ABIERTA", new DateTime(2026, 5, 3, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, 25, "SOLICITUD", "Ayuda para configurar un ordenador portátil", "Madrid" },
                    { 2, "Ofrezco ayuda para pasear perros por la zona durante las tardes.", "ABIERTA", new DateTime(2026, 5, 3, 11, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 8, 18, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, 15, "OFERTA", "Paseo de mascotas por la tarde", "Madrid" },
                    { 3, "Necesito ayuda para hacer una compra pequeña en el supermercado.", "ABIERTA", new DateTime(2026, 5, 3, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 6, 17, 30, 0, 0, DateTimeKind.Unspecified), 5, 3, 20, "SOLICITUD", "Recado para compra semanal", "Getafe" }
                });

            migrationBuilder.InsertData(
                table: "MovimientosPuntos",
                columns: new[] { "IdMovimiento", "Cantidad", "Descripcion", "FechaMovimiento", "IdPublicacion", "IdUsuario", "TipoMovimiento" },
                values: new object[,]
                {
                    { 2, 20, "Puntos obtenidos por realizar un recado.", new DateTime(2026, 5, 4, 9, 30, 0, 0, DateTimeKind.Unspecified), 3, 2, "GANANCIA" },
                    { 3, -20, "Puntos gastados por solicitar ayuda en un recado.", new DateTime(2026, 5, 4, 9, 30, 0, 0, DateTimeKind.Unspecified), 3, 3, "GASTO" }
                });

            migrationBuilder.InsertData(
                table: "SolicitudesParticipacion",
                columns: new[] { "IdSolicitud", "Estado", "FechaSolicitud", "IdPublicacion", "IdUsuarioSolicitante", "Mensaje" },
                values: new object[,]
                {
                    { 1, "PENDIENTE", new DateTime(2026, 5, 3, 12, 30, 0, 0, DateTimeKind.Unspecified), 1, 2, "Hola, puedo ayudarte con la configuración del ordenador." },
                    { 2, "ACEPTADA", new DateTime(2026, 5, 3, 13, 0, 0, 0, DateTimeKind.Unspecified), 3, 2, "Puedo hacer el recado esta semana." }
                });

            migrationBuilder.InsertData(
                table: "Valoraciones",
                columns: new[] { "IdValoracion", "Comentario", "FechaValoracion", "IdPublicacion", "IdUsuarioEmisor", "IdUsuarioReceptor", "Puntuacion" },
                values: new object[] { 1, "Muy amable y puntual.", new DateTime(2026, 5, 4, 10, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, 2, 5 });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosPuntos_IdPublicacion",
                table: "MovimientosPuntos",
                column: "IdPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosPuntos_IdUsuario",
                table: "MovimientosPuntos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_IdCategoria",
                table: "Publicaciones",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_IdUsuarioCreador",
                table: "Publicaciones",
                column: "IdUsuarioCreador");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesParticipacion_IdPublicacion_IdUsuarioSolicitante",
                table: "SolicitudesParticipacion",
                columns: new[] { "IdPublicacion", "IdUsuarioSolicitante" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesParticipacion_IdUsuarioSolicitante",
                table: "SolicitudesParticipacion",
                column: "IdUsuarioSolicitante");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_FirebaseUid",
                table: "Usuarios",
                column: "FirebaseUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdRol",
                table: "Usuarios",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_IdPublicacion",
                table: "Valoraciones",
                column: "IdPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_IdUsuarioEmisor",
                table: "Valoraciones",
                column: "IdUsuarioEmisor");

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_IdUsuarioReceptor",
                table: "Valoraciones",
                column: "IdUsuarioReceptor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosPuntos");

            migrationBuilder.DropTable(
                name: "SolicitudesParticipacion");

            migrationBuilder.DropTable(
                name: "Valoraciones");

            migrationBuilder.DropTable(
                name: "Publicaciones");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
