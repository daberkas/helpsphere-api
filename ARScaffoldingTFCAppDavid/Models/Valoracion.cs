namespace API_TFCAppDavid.Models
{
    public class Valoracion
    {
        public int IdValoracion { get; set; }
        public int Puntuacion { get; set; } // 1 a 5
        public string? Comentario { get; set; }
        public DateTime FechaValoracion { get; set; } = DateTime.Now;

        public int IdPublicacion { get; set; }
        public Publicacion? Publicacion { get; set; }

        public int IdUsuarioEmisor { get; set; }
        public Usuario? UsuarioEmisor { get; set; }

        public int IdUsuarioReceptor { get; set; }
        public Usuario? UsuarioReceptor { get; set; }
    }
}
