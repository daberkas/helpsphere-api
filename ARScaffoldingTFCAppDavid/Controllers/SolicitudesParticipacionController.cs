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
    public class SolicitudesParticipacionController : ControllerBase
    {
        private readonly ASPContext _context;

        public SolicitudesParticipacionController(ASPContext context)
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

        // GET: api/SolicitudesParticipacion
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolicitudParticipacion>>> GetSolicitudesParticipacion()
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var solicitudes = await _context.SolicitudesParticipacion
                .Include(s => s.Publicacion)
                .Include(s => s.UsuarioSolicitante)
                .Where(s =>
                    s.IdUsuarioSolicitante == usuario.IdUsuario ||
                    s.Publicacion.IdUsuarioCreador == usuario.IdUsuario
                )
                .Select(s => new
                {
                    s.IdSolicitud,
                    s.Mensaje,
                    s.Estado,
                    s.FechaSolicitud,
                    s.IdPublicacion,
                    s.IdUsuarioSolicitante,
                    TituloPublicacion = s.Publicacion.Titulo,
                    NombreSolicitante = s.UsuarioSolicitante.Nombre,
                    ApellidosSolicitante = s.UsuarioSolicitante.Apellidos
                })
                .ToListAsync();

            return Ok(solicitudes);
        }

        // GET: api/SolicitudesParticipacion/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudParticipacion>> GetSolicitudParticipacion(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var solicitud = await _context.SolicitudesParticipacion
                .Include(s => s.Publicacion)
                .Include(s => s.UsuarioSolicitante)
                .Where(s => s.IdSolicitud == id)
                .FirstOrDefaultAsync();

            if (solicitud == null)
            {
                return NotFound();
            }

            if (solicitud.IdUsuarioSolicitante != usuario.IdUsuario &&
                solicitud.Publicacion.IdUsuarioCreador != usuario.IdUsuario)
            {
                return Forbid();
            }

            return Ok(new
            {
                solicitud.IdSolicitud,
                solicitud.Mensaje,
                solicitud.Estado,
                solicitud.FechaSolicitud,
                solicitud.IdPublicacion,
                solicitud.IdUsuarioSolicitante,
                TituloPublicacion = solicitud.Publicacion.Titulo,
                NombreSolicitante = solicitud.UsuarioSolicitante.Nombre,
                ApellidosSolicitante = solicitud.UsuarioSolicitante.Apellidos
            });
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

            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var solicitudExistente = await _context.SolicitudesParticipacion
                .Include(s => s.Publicacion)
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitudExistente == null)
            {
                return NotFound();
            }

            // Solo el creador de la publicación puede aceptar/rechazar solicitudes
            if (solicitudExistente.Publicacion.IdUsuarioCreador != usuario.IdUsuario)
            {
                return Forbid();
            }

            var nuevoEstado = solicitudParticipacion.Estado?.ToUpper();

            if (nuevoEstado != "ACEPTADA" && nuevoEstado != "RECHAZADA")
            {
                return BadRequest("El estado solo puede cambiarse a ACEPTADA o RECHAZADA.");
            }

            solicitudExistente.Estado = nuevoEstado;

            if (nuevoEstado == "ACEPTADA")
            {
                var otrasSolicitudesPendientesOAceptadas = await _context.SolicitudesParticipacion
                    .Where(s =>
                        s.IdPublicacion == solicitudExistente.IdPublicacion &&
                        s.IdSolicitud != solicitudExistente.IdSolicitud &&
                        (s.Estado == "PENDIENTE" || s.Estado == "ACEPTADA"))
                    .ToListAsync();

                foreach (var solicitud in otrasSolicitudesPendientesOAceptadas)
                {
                    solicitud.Estado = "RECHAZADA";
                }

                solicitudExistente.Publicacion.Estado = "ASIGNADA";
            }
            else if (nuevoEstado == "RECHAZADA")
            {
                var hayOtraSolicitudAceptada = await _context.SolicitudesParticipacion
                    .AnyAsync(s =>
                        s.IdPublicacion == solicitudExistente.IdPublicacion &&
                        s.IdSolicitud != solicitudExistente.IdSolicitud &&
                        s.Estado == "ACEPTADA");

                if (solicitudExistente.Publicacion.Estado != "COMPLETADA" &&
                    solicitudExistente.Publicacion.Estado != "CANCELADA")
                {
                    solicitudExistente.Publicacion.Estado = hayOtraSolicitudAceptada
                        ? "ASIGNADA"
                        : "ABIERTA";
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/SolicitudesParticipacion
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SolicitudParticipacion>> PostSolicitudParticipacion(SolicitudParticipacion solicitudParticipacion)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.IdPublicacion == solicitudParticipacion.IdPublicacion);

            if (publicacion == null)
            {
                return NotFound("La publicación indicada no existe.");
            }

            if (publicacion.IdUsuarioCreador == usuario.IdUsuario)
            {
                return BadRequest("No puedes solicitar participar en tu propia publicación.");
            }

            // Si la publicación es una OFERTA, el usuario solicitante será quien reciba el servicio.
            // Por tanto, debe tener puntos suficientes para pagarlo.
            if (publicacion.TipoPublicacion == "OFERTA" &&
                usuario.SaldoPuntos < publicacion.PuntosEstimados)
            {
                return BadRequest(
                    $"No tienes saldo suficiente para aceptar esta oferta. Saldo actual: {usuario.SaldoPuntos} puntos."
                );
            }

            var solicitudDuplicada = await _context.SolicitudesParticipacion
                .AnyAsync(s =>
                    s.IdPublicacion == solicitudParticipacion.IdPublicacion &&
                    s.IdUsuarioSolicitante == usuario.IdUsuario
                );

            if (solicitudDuplicada)
            {
                return BadRequest("Ya has solicitado participar en esta publicación.");
            }

            solicitudParticipacion.IdUsuarioSolicitante = usuario.IdUsuario;
            solicitudParticipacion.Estado = "PENDIENTE";
            solicitudParticipacion.FechaSolicitud = DateTime.Now;

            _context.SolicitudesParticipacion.Add(solicitudParticipacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetSolicitudParticipacion",
                new { id = solicitudParticipacion.IdSolicitud },
                new
                {
                    solicitudParticipacion.IdSolicitud,
                    solicitudParticipacion.Mensaje,
                    solicitudParticipacion.Estado,
                    solicitudParticipacion.FechaSolicitud,
                    solicitudParticipacion.IdPublicacion,
                    solicitudParticipacion.IdUsuarioSolicitante
                }
            );
        }

        // DELETE: api/SolicitudesParticipacion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSolicitudParticipacion(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var solicitud = await _context.SolicitudesParticipacion
                .FirstOrDefaultAsync(s => s.IdSolicitud == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            // Solo el usuario que hizo la solicitud puede cancelarla/eliminarla
            if (solicitud.IdUsuarioSolicitante != usuario.IdUsuario)
            {
                return Forbid();
            }

            _context.SolicitudesParticipacion.Remove(solicitud);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SolicitudParticipacionExists(int id)
        {
            return _context.SolicitudesParticipacion.Any(e => e.IdSolicitud == id);
        }
    }
}
