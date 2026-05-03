namespace API_TFCAppDavid.Models
{
    public class MovimientoPuntos
    {

        public int IdMovimiento { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty; // GANANCIA / GASTO / AJUSTE
        public int Cantidad { get; set; } // Cantidad de puntos ganados o gastados
        public string? Descripcion { get; set; } // Descripción del movimiento
        public DateTime FechaMovimiento { get; set; } = DateTime.Now; // Fecha del movimiento

        public int IdUsuario { get; set; } // Clave foránea a Usuario
        public Usuario? Usuario { get; set; } // Propiedad de navegación a Usuario
        public int? IdPublicacion { get; set; } // Clave foránea a Publicacion (opcional, solo para movimientos relacionados con una publicación)
        public Publicacion? Publicacion { get; set; } // Propiedad de navegación a Publicacion (opcional)

    }
}
