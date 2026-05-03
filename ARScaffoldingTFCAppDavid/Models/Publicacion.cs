namespace API_TFCAppDavid.Models
{
    public class Publicacion
    {
        public int IdPublicacion { get; set; }

        public string TipoPublicacion { get; set; } = string.Empty; // SOLICITUD o OFERTA
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Zona { get; set; }
        public int PuntosEstimados { get; set; }
        public string Estado { get; set; } = "ABIERTA"; // ABIERTA, ASIGNADA, COMPLETADA, CANCELADA
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaServicio { get; set; }

        public int IdUsuarioCreador { get; set; }
        public Usuario? UsuarioCreador { get; set; }

        public int IdCategoria { get; set; }
        public Categoria? Categoria { get; set; }

        public ICollection<SolicitudParticipacion> Solicitudes { get; set; } = new List<SolicitudParticipacion>(); // Relación 1 a N con SolicitudParticipacion
        public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>(); // Relación 1 a N con Valoracion
        public ICollection<MovimientoPuntos> MovimientosPuntos { get; set; } = new List<MovimientoPuntos>(); // Relación 1 a N con MovimientoPuntos

    }
}
