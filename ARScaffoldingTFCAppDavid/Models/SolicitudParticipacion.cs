
namespace API_TFCAppDavid.Models
{
    public class SolicitudParticipacion
    {
        public int IdSolicitud { get; set; }
        public string? Mensaje { get; set; }
        public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, ACEPTADA, RECHAZADA, CANCELADA
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public int IdPublicacion { get; set; }
        public Publicacion? Publicacion { get; set; }

        public int IdUsuarioSolicitante { get; set; }
        public Usuario? UsuarioSolicitante { get; set; }

    }
}
