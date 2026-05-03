namespace API_TFCAppDavid.Models
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>(); // Relación 1 a N con Usuario
    }
}
