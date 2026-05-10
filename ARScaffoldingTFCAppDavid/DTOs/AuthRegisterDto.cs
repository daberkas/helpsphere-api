namespace API_TFCAppDavid.DTOs
{
    public class AuthRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Apellidos { get; set; }
        public string? Zona { get; set; }
    }
}
