using API_TFCAppDavid.Models;
using Microsoft.EntityFrameworkCore;

namespace API_TFCAppDavid.Contexto
{
    public class ASPContext : DbContext
    {
        public ASPContext(DbContextOptions<ASPContext> options) : base(options)
        {

        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Publicacion> Publicaciones { get; set; }
        public DbSet<SolicitudParticipacion> SolicitudesParticipacion { get; set; }
        public DbSet<Valoracion> Valoraciones { get; set; }
        public DbSet<MovimientoPuntos> MovimientosPuntos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>()
                .HasKey(r => r.IdRol); // Clave primaria para Rol

            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.IdUsuario); // Clave primaria para Usuario

            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.IdCategoria); // Clave primaria para Categoria

            modelBuilder.Entity<Publicacion>()
                .HasKey(p => p.IdPublicacion); // Clave primaria para Publicacion

            modelBuilder.Entity<SolicitudParticipacion>()
                .HasKey(s => s.IdSolicitud); // Clave primaria para SolicitudParticipacion

            modelBuilder.Entity<Valoracion>()
                .HasKey(v => v.IdValoracion); // Clave primaria para Valoracion

            modelBuilder.Entity<MovimientoPuntos>()
                .HasKey(mp => mp.IdMovimiento); // Clave primaria para MovimientoPuntos

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.FirebaseUid)
                .IsUnique(); // Índice único para FirebaseUid en Usuario

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique(); // Índice único para Email en Usuario

            modelBuilder.Entity<Usuario>()
                .Property(u => u.ReputacionMedia)
                .HasPrecision(3, 2); // Configuración de precisión para ReputacionMedia en Usuario

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol) // Relación Usuario - Rol
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

            modelBuilder.Entity<Publicacion>()
                .HasOne(p => p.UsuarioCreador)
                .WithMany(u => u.PublicacionesCreadas)
                .HasForeignKey(p => p.IdUsuarioCreador) // Relación Publicacion - Usuario
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

            modelBuilder.Entity<Publicacion>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Publicaciones)
                .HasForeignKey(p => p.IdCategoria) // Relación Publicacion - Categoria
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada

            modelBuilder.Entity<SolicitudParticipacion>()
                .HasOne(s => s.Publicacion)
                .WithMany(p => p.Solicitudes)
                .HasForeignKey(s => s.IdPublicacion) // Relación SolicitudParticipacion - Publicacion
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada, ya que no queremos eliminar las solicitudes si se elimina la publicación

            modelBuilder.Entity<SolicitudParticipacion>()
                .HasOne(s => s.UsuarioSolicitante)
                .WithMany(u => u.SolicitudesRealizadas)
                .HasForeignKey(s => s.IdUsuarioSolicitante)
                .OnDelete(DeleteBehavior.Restrict); // Relación SolicitudParticipacion - Usuario (Solicitante)

            modelBuilder.Entity<SolicitudParticipacion>()
                .HasIndex(s => new { s.IdPublicacion, s.IdUsuarioSolicitante })
                .IsUnique(); // Índice único para evitar que un usuario solicite participar más de una vez en la misma publicación

            modelBuilder.Entity<Valoracion>()
                .HasOne(v => v.Publicacion)
                .WithMany(p => p.Valoraciones)
                .HasForeignKey(v => v.IdPublicacion) // Relación Valoracion - Publicacion
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada, ya que no queremos eliminar las valoraciones si se elimina la publicación

            modelBuilder.Entity<Valoracion>()
                .HasOne(v => v.UsuarioEmisor)
                .WithMany(u => u.ValoracionesEmitidas)
                .HasForeignKey(v => v.IdUsuarioEmisor)
                .OnDelete(DeleteBehavior.Restrict); // Relación Valoracion - Usuario (Emisor)

            modelBuilder.Entity<Valoracion>()
                .HasOne(v => v.UsuarioReceptor)
                .WithMany(u => u.ValoracionesRecibidas)
                .HasForeignKey(v => v.IdUsuarioReceptor)
                .OnDelete(DeleteBehavior.Restrict); // Relación Valoracion - Usuario (Receptor)

            modelBuilder.Entity<MovimientoPuntos>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientoPuntos)
                .HasForeignKey(m => m.IdUsuario) // Relación MovimientoPuntos - Usuario
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada, ya que no queremos eliminar los movimientos de puntos si se elimina el usuario

            modelBuilder.Entity<MovimientoPuntos>()
                .HasOne(m => m.Publicacion)
                .WithMany(p => p.MovimientosPuntos)
                .HasForeignKey(m => m.IdPublicacion) // Relación MovimientoPuntos - Publicacion
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminación en cascada, ya que no queremos eliminar los movimientos de puntos si se elimina la publicación

            modelBuilder.Entity<Rol>().HasData(
                    new Rol { IdRol = 1, NombreRol = "Administrador" },
                    new Rol { IdRol = 2, NombreRol = "Usuario" }
                ); // Datos iniciales para la tabla Rol

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { IdCategoria = 1, Nombre = "Personas mayores", Descripcion = "Ayuda, acompañamiento y recados para personas mayores" },
                new Categoria { IdCategoria = 2, Nombre = "Mascotas", Descripcion = "Paseo y cuidado puntual de animales domésticos" },
                new Categoria { IdCategoria = 3, Nombre = "Informática", Descripcion = "Ayuda con móviles, ordenadores y programas" },
                new Categoria { IdCategoria = 4, Nombre = "Clases", Descripcion = "Apoyo educativo o formación básica" },
                new Categoria { IdCategoria = 5, Nombre = "Recados", Descripcion = "Compras, gestiones y pequeñas tareas cotidianas" },
                new Categoria { IdCategoria = 6, Nombre = "Reparaciones", Descripcion = "Pequeñas reparaciones domésticas" }
            );// Datos iniciales para la tabla Categoria

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    FirebaseUid = "firebase-admin-demo-001",
                    Nombre = "David",
                    Apellidos = "Bermudez",
                    Email = "admin@helpsphere.local",
                    Telefono = "600111222",
                    FotoPerfil = null,
                    Zona = "Cordoba",
                    Descripcion = "Usuario administrador inicial para pruebas.",
                    SaldoPuntos = 100,
                    ReputacionMedia = 5.00m,
                    FechaRegistro = new DateTime(2026, 5, 3, 10, 0, 0),
                    Activo = true,
                    IdRol = 1
                },
                new Usuario
                {
                    IdUsuario = 2,
                    FirebaseUid = "firebase-user-demo-002",
                    Nombre = "Laura",
                    Apellidos = "García",
                    Email = "laura@helpsphere.local",
                    Telefono = "600222333",
                    FotoPerfil = null,
                    Zona = "Cordoba",
                    Descripcion = "Usuaria de prueba interesada en ofrecer ayuda.",
                    SaldoPuntos = 50,
                    ReputacionMedia = 4.50m,
                    FechaRegistro = new DateTime(2026, 5, 3, 10, 15, 0),
                    Activo = true,
                    IdRol = 2
                },
                new Usuario
                {
                    IdUsuario = 3,
                    FirebaseUid = "firebase-user-demo-003",
                    Nombre = "Miguel",
                    Apellidos = "Sánchez",
                    Email = "miguel@helpsphere.local",
                    Telefono = "600333444",
                    FotoPerfil = null,
                    Zona = "Cordoba",
                    Descripcion = "Usuario de prueba que solicita ayuda puntual.",
                    SaldoPuntos = 30,
                    ReputacionMedia = 0.00m,
                    FechaRegistro = new DateTime(2026, 5, 3, 10, 30, 0),
                    Activo = true,
                    IdRol = 2
                }
            );// Datos iniciales para la tabla Usuario
            modelBuilder.Entity<Publicacion>().HasData(
                new Publicacion
                {
                    IdPublicacion = 1,
                    TipoPublicacion = "SOLICITUD",
                    Titulo = "Ayuda para configurar un ordenador portátil",
                    Descripcion = "Necesito ayuda para instalar programas básicos y configurar el correo electrónico.",
                    Zona = "Madrid",
                    PuntosEstimados = 25,
                    Estado = "ABIERTA",
                    FechaCreacion = new DateTime(2026, 5, 3, 11, 0, 0),
                    FechaServicio = new DateTime(2026, 5, 10, 10, 0, 0),
                    IdUsuarioCreador = 3,
                    IdCategoria = 3
                },
                new Publicacion
                {
                    IdPublicacion = 2,
                    TipoPublicacion = "OFERTA",
                    Titulo = "Paseo de mascotas por la tarde",
                    Descripcion = "Ofrezco ayuda para pasear perros por la zona durante las tardes.",
                    Zona = "Madrid",
                    PuntosEstimados = 15,
                    Estado = "ABIERTA",
                    FechaCreacion = new DateTime(2026, 5, 3, 11, 30, 0),
                    FechaServicio = new DateTime(2026, 5, 8, 18, 0, 0),
                    IdUsuarioCreador = 2,
                    IdCategoria = 2
                },
                new Publicacion
                {
                    IdPublicacion = 3,
                    TipoPublicacion = "SOLICITUD",
                    Titulo = "Recado para compra semanal",
                    Descripcion = "Necesito ayuda para hacer una compra pequeña en el supermercado.",
                    Zona = "Getafe",
                    PuntosEstimados = 20,
                    Estado = "ABIERTA",
                    FechaCreacion = new DateTime(2026, 5, 3, 12, 0, 0),
                    FechaServicio = new DateTime(2026, 5, 6, 17, 30, 0),
                    IdUsuarioCreador = 3,
                    IdCategoria = 5
                }
            );// Datos iniciales para la tabla Publicacion
            modelBuilder.Entity<SolicitudParticipacion>().HasData(
                new SolicitudParticipacion
                {
                    IdSolicitud = 1,
                    Mensaje = "Hola, puedo ayudarte con la configuración del ordenador.",
                    Estado = "PENDIENTE",
                    FechaSolicitud = new DateTime(2026, 5, 3, 12, 30, 0),
                    IdPublicacion = 1,
                    IdUsuarioSolicitante = 2
                },
                new SolicitudParticipacion
                {
                    IdSolicitud = 2,
                    Mensaje = "Puedo hacer el recado esta semana.",
                    Estado = "ACEPTADA",
                    FechaSolicitud = new DateTime(2026, 5, 3, 13, 0, 0),
                    IdPublicacion = 3,
                    IdUsuarioSolicitante = 2
                }
            );// Datos iniciales para la tabla SolicitudParticipacion
            modelBuilder.Entity<Valoracion>().HasData(
                new Valoracion
                {
                    IdValoracion = 1,
                    Puntuacion = 5,
                    Comentario = "Muy amable y puntual.",
                    FechaValoracion = new DateTime(2026, 5, 4, 10, 0, 0),
                    IdPublicacion = 3,
                    IdUsuarioEmisor = 3,
                    IdUsuarioReceptor = 2
                }
            );// Datos iniciales para la tabla Valoracion
            modelBuilder.Entity<MovimientoPuntos>().HasData(
                new MovimientoPuntos
                {
                    IdMovimiento = 1,
                    TipoMovimiento = "AJUSTE",
                    Cantidad = 100,
                    Descripcion = "Saldo inicial de prueba para administrador.",
                    FechaMovimiento = new DateTime(2026, 5, 3, 10, 0, 0),
                    IdUsuario = 1,
                    IdPublicacion = null
                },
                new MovimientoPuntos
                {
                    IdMovimiento = 2,
                    TipoMovimiento = "GANANCIA",
                    Cantidad = 20,
                    Descripcion = "Puntos obtenidos por realizar un recado.",
                    FechaMovimiento = new DateTime(2026, 5, 4, 9, 30, 0),
                    IdUsuario = 2,
                    IdPublicacion = 3
                },
                new MovimientoPuntos
                {
                    IdMovimiento = 3,
                    TipoMovimiento = "GASTO",
                    Cantidad = -20,
                    Descripcion = "Puntos gastados por solicitar ayuda en un recado.",
                    FechaMovimiento = new DateTime(2026, 5, 4, 9, 30, 0),
                    IdUsuario = 3,
                    IdPublicacion = 3
                }
            );// Datos iniciales para la tabla MovimientoPuntos
        }
    }
}

