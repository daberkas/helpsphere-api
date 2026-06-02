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
    public class PublicacionesController : ControllerBase
    {
        private readonly ASPContext _context;

        public PublicacionesController(ASPContext context)
        {
            _context = context;
        }

        private async Task<Usuario?> GetUsuarioAutenticado()
        {
            // Se obtiene Firebase UID desde el token JWT
            var firebaseUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("user_id")?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(firebaseUid))
            {
                return null;
            }

            // Se busca usuario interno en SQL Server
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid);
        }

        // GET: api/Publicaciones
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Publicacion>>> GetPublicaciones()
        {
            var publicaciones = await _context.Publicaciones
                .Select(p => new
                {
                    p.IdPublicacion,
                    p.TipoPublicacion,
                    p.Titulo,
                    p.Descripcion,
                    p.Zona,
                    p.PuntosEstimados,
                    p.Estado,
                    p.FechaCreacion,
                    p.FechaServicio,
                    p.IdUsuarioCreador,
                    p.IdCategoria
                })
                .ToListAsync();

            return Ok(publicaciones);
        }

        // GET: api/Publicaciones/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Publicacion>> GetPublicacion(int id)
        {
            var publicacion = await _context.Publicaciones
                .Where(p => p.IdPublicacion == id)
                .Select(p => new
                {
                    p.IdPublicacion,
                    p.TipoPublicacion,
                    p.Titulo,
                    p.Descripcion,
                    p.Zona,
                    p.PuntosEstimados,
                    p.Estado,
                    p.FechaCreacion,
                    p.FechaServicio,
                    p.IdUsuarioCreador,
                    p.IdCategoria
                })
                .FirstOrDefaultAsync();

            if (publicacion == null)
            {
                return NotFound();
            }

            return Ok(publicacion);
        }

        // PUT: api/Publicaciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublicacion(int id, Publicacion publicacion)
        {
            if (id != publicacion.IdPublicacion)
            {
                return BadRequest();
            }

            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            //Se busca publicación existente
            var publicacionExistente = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.IdPublicacion == id);

            if (publicacionExistente == null)
            {
                return NotFound();
            }

            var esCreador = publicacionExistente.IdUsuarioCreador == usuario.IdUsuario;

            var esUsuarioAceptado = await _context.SolicitudesParticipacion
                .AnyAsync(s =>
                    s.IdPublicacion == id &&
                    s.IdUsuarioSolicitante == usuario.IdUsuario &&
                    s.Estado == "ACEPTADA");

            var nuevoEstado = publicacion.Estado?.ToUpper();

            var cambioSoloEstadoFinal =
                nuevoEstado == "COMPLETADA" ||
                nuevoEstado == "CANCELADA";

            if (!esCreador && !(esUsuarioAceptado && cambioSoloEstadoFinal))
            {
                return Forbid();
            }

            if (esCreador)
            {
                publicacionExistente.TipoPublicacion = publicacion.TipoPublicacion;
                publicacionExistente.Titulo = publicacion.Titulo;
                publicacionExistente.Descripcion = publicacion.Descripcion;
                publicacionExistente.Zona = publicacion.Zona;
                publicacionExistente.PuntosEstimados = publicacion.PuntosEstimados;
                publicacionExistente.Estado = nuevoEstado;
                publicacionExistente.FechaServicio = publicacion.FechaServicio;
                publicacionExistente.IdCategoria = publicacion.IdCategoria;
            }
            else
            {
                publicacionExistente.Estado = nuevoEstado;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Publicaciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Publicacion>> PostPublicacion(Publicacion publicacion)
        {

            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            publicacion.TipoPublicacion = publicacion.TipoPublicacion?.ToUpper();
            publicacion.Estado = publicacion.Estado?.ToUpper();

            if (publicacion.TipoPublicacion != "SOLICITUD" && publicacion.TipoPublicacion != "OFERTA")
            {
                return BadRequest("El tipo de publicación debe ser SOLICITUD u OFERTA.");
            }

            if (publicacion.PuntosEstimados <= 0)
            {
                return BadRequest("Los puntos estimados deben ser mayores que cero.");
            }

            if (publicacion.TipoPublicacion == "SOLICITUD" &&
                usuario.SaldoPuntos < publicacion.PuntosEstimados)
            {
                return BadRequest(
                    $"No tienes saldo suficiente para crear esta solicitud. Saldo actual: {usuario.SaldoPuntos} puntos."
                );
            }

            // Se asigna automáticamente el creador
            publicacion.IdUsuarioCreador = usuario.IdUsuario;

            // Fecha automática
            publicacion.FechaCreacion = DateTime.Now;

            _context.Publicaciones.Add(publicacion);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetPublicacion",
                new { id = publicacion.IdPublicacion },
                new
                {
                    publicacion.IdPublicacion,
                    publicacion.TipoPublicacion,
                    publicacion.Titulo,
                    publicacion.Descripcion,
                    publicacion.Zona,
                    publicacion.PuntosEstimados,
                    publicacion.Estado,
                    publicacion.FechaCreacion,
                    publicacion.FechaServicio,
                    publicacion.IdUsuarioCreador,
                    publicacion.IdCategoria
                }
            );

        }

        // DELETE: api/Publicaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublicacion(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            //Se busca publicación existente
            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.IdPublicacion == id);

            if (publicacion == null)
            {
                return NotFound();
            }

            //Solo el creador de la publicación puede eliminarla
            if (publicacion.IdUsuarioCreador != usuario.IdUsuario)
            {
                return Forbid();
            }

            // No permitir eliminar publicaciones con actividad asociada

            var tieneSolicitudes = await _context.SolicitudesParticipacion
                .AnyAsync(s => s.IdPublicacion == id);

            var tieneValoraciones = await _context.Valoraciones
                .AnyAsync(v => v.IdPublicacion == id);

            var tieneMovimientos = await _context.MovimientosPuntos
                .AnyAsync(m => m.IdPublicacion == id);

            if (tieneSolicitudes || tieneValoraciones || tieneMovimientos)
            {
                return BadRequest(
                    "No se puede eliminar una publicación que tiene solicitudes, valoraciones o movimientos de puntos asociados. Cambie el estado a CANCELADA si desea dejarla inactiva."
                );
            }

            _context.Publicaciones.Remove(publicacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
