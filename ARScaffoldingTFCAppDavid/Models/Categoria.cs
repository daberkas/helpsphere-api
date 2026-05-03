namespace API_TFCAppDavid.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>(); // Relación 1 a N con Publicacion
    }
}
