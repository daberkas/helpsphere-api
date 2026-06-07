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
    /// <summary>
    /// Gestión de usuarios registrados en el sistema.
    /// Permite consultar la información del perfil del usuario autenticado, 
    /// así como modificar su información personal o eliminar su cuenta (desactivándola).
    /// </summary>
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

        /// <summary>
        /// Comprueba si el usuario autenticado tiene rol de administrador (IdRol = 1).
        /// </summary>
        private async Task<bool> EsAdministrador()
        {
            var usuario = await GetUsuarioAutenticado();
            return usuario != null && usuario.IdRol == 1;
        }

        /// <summary>
        /// Obtiene el usuario autenticado a partir de su FirebaseUid, 
        /// que se extrae de los claims del token JWT.
        /// </summary>
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

        /// <summary>
        /// Obtiene el listado de todos los usuarios registrados en el sistema.
        /// Operación restringida a administradores, ya que expone información sensible 
        /// de los usuarios.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Solo los administradores pueden acceder al listado completo de usuarios
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

        /// <summary>
        /// Obtiene la información del perfil del usuario autenticado, 
        /// a partir de su IdUsuario.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            // Evitar que un usuario pueda acceder a la información de otro usuario,
            // incluso si es administrador
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

        /// <summary>
        /// Actualiza los datos personales del perfil del usuario autenticado, 
        /// a partir de su IdUsuario.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            // Evitar que un usuario pueda modificar la información de otro usuario,
            // Solo el propio usuario puede modificar su información,
            // incluso si es administrador
            if (usuarioAutenticado.IdUsuario != id)
            {
                return Forbid();
            }

            // Solo se permiten modificar ciertos campos del perfil,
            // no el email, rol, saldo de puntos, etc.
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos editables del perfil
            usuarioExistente.Nombre = usuario.Nombre;
            usuarioExistente.Apellidos = usuario.Apellidos;
            usuarioExistente.Telefono = usuario.Telefono;
            usuarioExistente.FotoPerfil = usuario.FotoPerfil;
            usuarioExistente.Zona = usuario.Zona;
            usuarioExistente.Descripcion = usuario.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Permite a un administrador cambiar el estado de un usuario para bloquearlo (activo/inactivo).
        /// Incluye validaciones para evitar que un administrador se bloquee a sí mismo o que el sistema 
        /// se quede sin administradores activos.
        /// </summary>
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstadoUsuario(int id, Usuario usuario)
        {
            var usuarioAutenticado = await GetUsuarioAutenticado();

            if (usuarioAutenticado == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            // Verifico que la operacción la realice un administrador.
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

        /// <summary>
        /// Operación no permitida. Los usuarios se crean automáticamente al registrarse mediante 
        /// el controlador de autenticación AuthController.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            return BadRequest("Los usuarios deben crearse mediante el controlador de autenticación.");
        }

        /// <summary>
        /// Desactiva la cuenta del usuario autenticado, a partir de su IdUsuario, 
        /// en lugar de eliminarla físicamente de la base de datos.
        /// </summary>
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

            // En lugar de eliminar físicamente el usuario,
            // se marca como inactivo para preservar la integridad referencial
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
