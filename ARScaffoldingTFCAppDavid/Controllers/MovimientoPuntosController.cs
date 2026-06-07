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
    /// Consulta de movimientos de puntos del usuario autenticado.
    /// Los movimientos de puntos se generan automáticamente 
    /// en función de la logica de negocio y no pueden ser creados, 
    /// modificados o eliminados manualmente desde el cliente.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoPuntosController : ControllerBase
    {
        private readonly ASPContext _context;

        public MovimientoPuntosController(ASPContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el usuario autenticado a partir del token JWT y su Firebase UID.
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
        /// Obtiene la lista de movimientos de puntos del usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoPuntos>>> GetMovimientosPuntos()
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            // Solo se devuelven los movimientos de puntos del usuario autenticado
            var movimientos = await _context.MovimientosPuntos
                .Where(m => m.IdUsuario == usuario.IdUsuario)
                .Select(m => new
                {
                    m.IdMovimiento,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.Descripcion,
                    m.FechaMovimiento,
                    m.IdUsuario,
                    m.IdPublicacion
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        /// <summary>
        /// Obtiene un movimiento de puntos específico por su ID, 
        /// solo si pertenece al usuario autenticado.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoPuntos>> GetMovimientoPuntos(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var movimiento = await _context.MovimientosPuntos
                .Where(m => m.IdMovimiento == id)
                .Select(m => new
                {
                    m.IdMovimiento,
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.Descripcion,
                    m.FechaMovimiento,
                    m.IdUsuario,
                    m.IdPublicacion
                })
                .FirstOrDefaultAsync();

            if (movimiento == null)
            {
                return NotFound();
            }

            // Verifico que el movimiento de puntos pertenece al usuario autenticado,
            // impidiendo el acceso a movimientos de otros usuarios
            if (movimiento.IdUsuario != usuario.IdUsuario)
            {
                return Forbid();
            }

            return Ok(movimiento);
        }

        /// <summary>
        /// Operación no permitida: Los movimientos de puntos no se modifican manualmente 
        /// desde el cliente. Son generados automáticamente.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovimientoPuntos(int id, MovimientoPuntos movimientoPuntos)
        {
            return BadRequest("Los movimientos de puntos no se modifican manualmente desde el cliente.");
        }

        /// <summary>
        /// Operación no permitida: Los movimientos de puntos no se crean manualmente.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MovimientoPuntos>> PostMovimientoPuntos(MovimientoPuntos movimientoPuntos)
        {
            return BadRequest("Los movimientos de puntos no se crean manualmente desde el cliente.");
        }

        /// <summary>
        /// Operación no permitida: Los movimientos de puntos no se eliminan manualmente.
        /// Forman parte del historial de transacciones del usuario y se mantienen para referencia futura.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimientoPuntos(int id)
        {
            return BadRequest("Los movimientos de puntos no se eliminan manualmente desde el cliente.");
        }

        private bool MovimientoPuntosExists(int id)
        {
            return _context.MovimientosPuntos.Any(e => e.IdMovimiento == id);
        }
    }
}
