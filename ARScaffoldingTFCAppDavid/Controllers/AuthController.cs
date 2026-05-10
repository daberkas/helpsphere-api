using API_TFCAppDavid.Contexto;
using API_TFCAppDavid.DTOs;
using API_TFCAppDavid.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace API_TFCAppDavid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ASPContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthController(
            ASPContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRegisterDto dto)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var client = _httpClientFactory.CreateClient();

            var firebaseRequest = new
            {
                email = dto.Email,
                password = dto.Password,
                returnSecureToken = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(firebaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(responseBody);
            }

            using var jsonDoc = JsonDocument.Parse(responseBody);
            var firebaseUid = jsonDoc.RootElement.GetProperty("localId").GetString();
            var idToken = jsonDoc.RootElement.GetProperty("idToken").GetString();

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return BadRequest("Error al recuperar la identidad del usuario.");
            }

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.FirebaseUid == firebaseUid || u.Email == dto.Email);

            if (!usuarioExiste)
            {
                var usuario = new Usuario
                {
                    FirebaseUid = firebaseUid,
                    Nombre = dto.Nombre,
                    Apellidos = dto.Apellidos,
                    Email = dto.Email,
                    Zona = dto.Zona,
                    SaldoPuntos = 0,
                    ReputacionMedia = 0,
                    FechaRegistro = DateTime.Now,
                    Activo = true,
                    IdRol = 2
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Usuario registrado correctamente",
                firebaseUid,
                idToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLoginDto dto)
        {
            var apiKey = _configuration["Firebase:ApiKey"];
            var client = _httpClientFactory.CreateClient();

            var firebaseRequest = new
            {
                email = dto.Email,
                password = dto.Password,
                returnSecureToken = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(firebaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(responseBody);
            }

            using var jsonDoc = JsonDocument.Parse(responseBody);
            var firebaseUid = jsonDoc.RootElement.GetProperty("localId").GetString();
            var idToken = jsonDoc.RootElement.GetProperty("idToken").GetString();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);

            return Ok(new
            {
                message = "Inicio de sesión correcto",
                firebaseUid,
                idToken,
                usuario
            });
        }
    }
}