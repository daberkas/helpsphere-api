
namespace API_TFCAppDavid.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string FirebaseUid { get; set; } = string.Empty; // UID autenticado desde Firebase
        public string Nombre { get; set; } = string.Empty;
        public string? Apellidos { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? FotoPerfil { get; set; }
        public string? Zona { get; set; }
        public string? Descripcion { get; set; }

        public int SaldoPuntos { get; set; } = 0; // Puntos acumulados por el usuario
        public decimal ReputacionMedia { get; set; } = 0.0m; // Reputación media basada en valoraciones recibidas
        public DateTime FechaRegistro { get; set; } = DateTime.Now; // Fecha de registro del usuario
        public bool Activo {get; set; } = true; // Indica si el usuario está activo o inactivo
        
        public int IdRol { get; set; } // Clave foránea a Rol
        public Rol? Rol { get; set; } // Propiedad de navegación a Rol

        public ICollection<Publicacion> PublicacionesCreadas { get; set; } = new List<Publicacion>(); // Publicaciones creadas por el usuario (relacion 1 a N)
        public ICollection<SolicitudParticipacion> SolicitudesRealizadas { get; set; } = new List<SolicitudParticipacion>(); // Solicitudes de participación realizadas por el usuario (relacion 1 a N)
        public ICollection<Valoracion> ValoracionesEmitidas { get; set; } = new List<Valoracion>(); // Valoraciones emitidas por el usuario (relacion 1 a N)
        public ICollection<Valoracion> ValoracionesRecibidas { get; set; } = new List<Valoracion>(); // Valoraciones recibidas por el usuario (relacion 1 a N)
        public ICollection<MovimientoPuntos> MovimientoPuntos { get; set; } = new List<MovimientoPuntos>(); // Movimientos de puntos del usuario (relacion 1 a N)


    }
}
