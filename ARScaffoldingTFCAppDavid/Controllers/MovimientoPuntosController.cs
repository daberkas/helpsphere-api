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
    public class MovimientoPuntosController : ControllerBase
    {
        private readonly ASPContext _context;

        public MovimientoPuntosController(ASPContext context)
        {
            _context = context;
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

        // GET: api/MovimientoPuntos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoPuntos>>> GetMovimientosPuntos()
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

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

        // GET: api/MovimientoPuntos/5
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

            if (movimiento.IdUsuario != usuario.IdUsuario)
            {
                return Forbid();
            }

            return Ok(movimiento);
        }

        // PUT: api/MovimientoPuntos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovimientoPuntos(int id, MovimientoPuntos movimientoPuntos)
        {
            return BadRequest("Los movimientos de puntos no se modifican manualmente desde el cliente.");
        }

        // POST: api/MovimientoPuntos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MovimientoPuntos>> PostMovimientoPuntos(MovimientoPuntos movimientoPuntos)
        {
            return BadRequest("Los movimientos de puntos no se crean manualmente desde el cliente.");
        }

        // DELETE: api/MovimientoPuntos/5
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
