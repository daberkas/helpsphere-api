using API_TFCAppDavid.Contexto;
using API_TFCAppDavid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace API_TFCAppDavid.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ASPContext _context;

        public UsuariosController(ASPContext context)
        {
            _context = context;
        }

        private async Task<bool> EsAdministrador()
        {
            var usuario = await GetUsuarioAutenticado();
            return usuario != null && usuario.IdRol == 1;
        }

        private async Task<Usuario?> GetUsuarioAutenticado()
        {
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("user_id")?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            if (!await EsAdministrador())
            {
                return Forbid();
            }

            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellidos,
                    u.Email,
                    u.Zona,
                    u.Descripcion,
                    u.SaldoPuntos,
                    u.ReputacionMedia,
                    u.FechaRegistro,
                    u.Activo,
                    u.IdRol
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            if (usuarioAutenticado.IdUsuario != id)
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellidos,
                    u.Email,
                    u.Telefono,
                    u.FotoPerfil,
                    u.Zona,
                    u.Descripcion,
                    u.SaldoPuntos,
                    u.ReputacionMedia,
                    u.FechaRegistro,
                    u.Activo,
                    u.IdRol
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // PUT: api/Usuarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            if (usuarioAutenticado.IdUsuario != id)
            {
                return Forbid();
            }

            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Apellidos = usuario.Apellidos;
            usuarioExistente.Telefono = usuario.Telefono;
            usuarioExistente.FotoPerfil = usuario.FotoPerfil;
            usuarioExistente.Zona = usuario.Zona;
            usuarioExistente.Descripcion = usuario.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id, Usuario usuario)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            if (usuarioAutenticado.IdRol != 1)
            {
                return Forbid();
            }

            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Para evitar que un administrador se bloquee a sí mismo
            if (usuarioAutenticado.IdUsuario == id && usuario.Activo == false)
            {
                return BadRequest("No puedes bloquear tu propia cuenta de administrador.");
            }

            // Para evitar que el sistema se pueda quedar sin administradores activos
            if (usuarioExistente.IdRol == 1 && usuario.Activo == false)
            {
                var numeroAdminsActivos = await _context.Usuarios
                    .CountAsync(u => u.IdRol == 1 && u.Activo);

                if (numeroAdminsActivos <= 1)
                {
                    return BadRequest("Debe existir al menos un administrador activo en el sistema.");
                }
            }

            usuarioExistente.Activo = usuario.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Usuarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            return BadRequest("Los usuarios deben crearse mediante el controlador de autenticación.");
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            if (usuarioAutenticado.IdUsuario != id)
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
