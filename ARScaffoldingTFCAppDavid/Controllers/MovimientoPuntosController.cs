using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_TFCAppDavid.Contexto;
using API_TFCAppDavid.Models;

namespace API_TFCAppDavid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoPuntosController : ControllerBase
    {
        private readonly ASPContext _context;

        public MovimientoPuntosController(ASPContext context)
        {
            _context = context;
        }

        // GET: api/MovimientoPuntos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovimientoPuntos>>> GetMovimientosPuntos()
        {
            return await _context.MovimientosPuntos.ToListAsync();
        }

        // GET: api/MovimientoPuntos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MovimientoPuntos>> GetMovimientoPuntos(int id)
        {
            var movimientoPuntos = await _context.MovimientosPuntos.FindAsync(id);

            if (movimientoPuntos == null)
            {
                return NotFound();
            }

            return movimientoPuntos;
        }

        // PUT: api/MovimientoPuntos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovimientoPuntos(int id, MovimientoPuntos movimientoPuntos)
        {
            if (id != movimientoPuntos.IdMovimiento)
            {
                return BadRequest();
            }

            _context.Entry(movimientoPuntos).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovimientoPuntosExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MovimientoPuntos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MovimientoPuntos>> PostMovimientoPuntos(MovimientoPuntos movimientoPuntos)
        {
            _context.MovimientosPuntos.Add(movimientoPuntos);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMovimientoPuntos", new { id = movimientoPuntos.IdMovimiento }, movimientoPuntos);
        }

        // DELETE: api/MovimientoPuntos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovimientoPuntos(int id)
        {
            var movimientoPuntos = await _context.MovimientosPuntos.FindAsync(id);
            if (movimientoPuntos == null)
            {
                return NotFound();
            }

            _context.MovimientosPuntos.Remove(movimientoPuntos);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovimientoPuntosExists(int id)
        {
            return _context.MovimientosPuntos.Any(e => e.IdMovimiento == id);
        }
    }
}
