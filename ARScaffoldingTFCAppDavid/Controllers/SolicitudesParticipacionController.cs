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
    public class SolicitudesParticipacionController : ControllerBase
    {
        private readonly ASPContext _context;

        public SolicitudesParticipacionController(ASPContext context)
        {
            _context = context;
        }

        // GET: api/SolicitudesParticipacion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolicitudParticipacion>>> GetSolicitudesParticipacion()
        {
            return await _context.SolicitudesParticipacion.ToListAsync();
        }

        // GET: api/SolicitudesParticipacion/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudParticipacion>> GetSolicitudParticipacion(int id)
        {
            var solicitudParticipacion = await _context.SolicitudesParticipacion.FindAsync(id);

            if (solicitudParticipacion == null)
            {
                return NotFound();
            }

            return solicitudParticipacion;
        }

        // PUT: api/SolicitudesParticipacion/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSolicitudParticipacion(int id, SolicitudParticipacion solicitudParticipacion)
        {
            if (id != solicitudParticipacion.IdSolicitud)
            {
                return BadRequest();
            }

            _context.Entry(solicitudParticipacion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SolicitudParticipacionExists(id))
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

        // POST: api/SolicitudesParticipacion
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SolicitudParticipacion>> PostSolicitudParticipacion(SolicitudParticipacion solicitudParticipacion)
        {
            _context.SolicitudesParticipacion.Add(solicitudParticipacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSolicitudParticipacion", new { id = solicitudParticipacion.IdSolicitud }, solicitudParticipacion);
        }

        // DELETE: api/SolicitudesParticipacion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSolicitudParticipacion(int id)
        {
            var solicitudParticipacion = await _context.SolicitudesParticipacion.FindAsync(id);
            if (solicitudParticipacion == null)
            {
                return NotFound();
            }

            _context.SolicitudesParticipacion.Remove(solicitudParticipacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SolicitudParticipacionExists(int id)
        {
            return _context.SolicitudesParticipacion.Any(e => e.IdSolicitud == id);
        }
    }
}
