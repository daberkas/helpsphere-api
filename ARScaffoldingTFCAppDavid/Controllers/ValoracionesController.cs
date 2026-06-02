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
    public class ValoracionesController : ControllerBase
    {
        private readonly ASPContext _context;

        public ValoracionesController(ASPContext context)
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

        // GET: api/Valoraciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Valoracion>>> GetValoraciones()
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var valoraciones = await _context.Valoraciones
                .Where(v =>
                    v.IdUsuarioEmisor == usuario.IdUsuario ||
                    v.IdUsuarioReceptor == usuario.IdUsuario
                )
                .Select(v => new
                {
                    v.IdValoracion,
                    v.Puntuacion,
                    v.Comentario,
                    v.FechaValoracion,
                    v.IdPublicacion,
                    TituloPublicacion = v.Publicacion.Titulo,
                    v.IdUsuarioEmisor,
                    NombreEmisor = v.UsuarioEmisor.Nombre,
                    ApellidosEmisor = v.UsuarioEmisor.Apellidos,
                    v.IdUsuarioReceptor,
                    NombreReceptor = v.UsuarioReceptor.Nombre,
                    ApellidosReceptor = v.UsuarioReceptor.Apellidos
                })
                .ToListAsync();

            return Ok(valoraciones);
        }

        // GET: api/Valoraciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Valoracion>> GetValoracion(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var valoracion = await _context.Valoraciones
                .Where(v => v.IdValoracion == id)
                .Select(v => new
                {
                    v.IdValoracion,
                    v.Puntuacion,
                    v.Comentario,
                    v.FechaValoracion,
                    v.IdPublicacion,
                    TituloPublicacion = v.Publicacion.Titulo,
                    v.IdUsuarioEmisor,
                    NombreEmisor = v.UsuarioEmisor.Nombre,
                    ApellidosEmisor = v.UsuarioEmisor.Apellidos,
                    v.IdUsuarioReceptor,
                    NombreReceptor = v.UsuarioReceptor.Nombre,
                    ApellidosReceptor = v.UsuarioReceptor.Apellidos
                })
                .FirstOrDefaultAsync();

            if (valoracion == null)
            {
                return NotFound();
            }

            if (valoracion.IdUsuarioEmisor != usuario.IdUsuario &&
                valoracion.IdUsuarioReceptor != usuario.IdUsuario)
            {
                return Forbid();
            }

            return Ok(valoracion);
        }

        // PUT: api/Valoraciones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutValoracion(int id, Valoracion valoracion)
        {
            if (id != valoracion.IdValoracion)
            {
                return BadRequest();
            }

            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var valoracionExistente = await _context.Valoraciones
                .FirstOrDefaultAsync(v => v.IdValoracion == id);

            if (valoracionExistente == null)
            {
                return NotFound();
            }

            // Solo el usuario que emitió la valoración puede editarla
            if (valoracionExistente.IdUsuarioEmisor != usuario.IdUsuario)
            {
                return Forbid();
            }

            valoracionExistente.Puntuacion = valoracion.Puntuacion;
            valoracionExistente.Comentario = valoracion.Comentario;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Valoraciones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Valoracion>> PostValoracion(Valoracion valoracion)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.IdPublicacion == valoracion.IdPublicacion);

            if (publicacion == null)
            {
                return NotFound("La publicación indicada no existe.");
            }

            if (valoracion.IdUsuarioReceptor == usuario.IdUsuario)
            {
                return BadRequest("No puedes valorarte a ti mismo.");
            }

            var receptorExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == valoracion.IdUsuarioReceptor);

            if (!receptorExiste)
            {
                return NotFound("El usuario receptor no existe.");
            }

            var valoracionDuplicada = await _context.Valoraciones
                .AnyAsync(v =>
                    v.IdPublicacion == valoracion.IdPublicacion &&
                    v.IdUsuarioEmisor == usuario.IdUsuario &&
                    v.IdUsuarioReceptor == valoracion.IdUsuarioReceptor
                );

            if (valoracionDuplicada)
            {
                return BadRequest("Ya has valorado a este usuario en esta publicación.");
            }

            var usuarioReceptor = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == valoracion.IdUsuarioReceptor);

            if (usuarioReceptor == null)
            {
                return NotFound("El usuario receptor no existe.");
            }

            if (publicacion.Estado != "COMPLETADA")
            {
                return BadRequest("Solo se pueden valorar publicaciones completadas.");
            }

            var puntos = publicacion.PuntosEstimados;

            valoracion.IdUsuarioEmisor = usuario.IdUsuario;
            valoracion.FechaValoracion = DateTime.Now;

            Usuario usuarioQuePaga;
            Usuario usuarioQueGana;

            if (publicacion.TipoPublicacion == "SOLICITUD")
            {
                // El creador solicitó ayuda, por tanto paga.
                usuarioQuePaga = usuario;
                usuarioQueGana = usuarioReceptor;
            }
            else if (publicacion.TipoPublicacion == "OFERTA")
            {
                // El creador ofreció ayuda, por tanto gana.
                usuarioQuePaga = usuario;
                usuarioQueGana = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.IdUsuario == publicacion.IdUsuarioCreador);

                if (usuarioQueGana == null)
                {
                    return NotFound("El usuario creador de la oferta no existe.");
                }
            }
            else
            {
                return BadRequest("Tipo de publicación no válido.");
            }

            if (usuarioQuePaga.SaldoPuntos < puntos)
            {
                return BadRequest(
                    $"Saldo insuficiente. El usuario dispone de {usuarioQuePaga.SaldoPuntos} puntos y necesita {puntos}."
                );
            }

            usuarioQuePaga.SaldoPuntos -= puntos;
            usuarioQueGana.SaldoPuntos += puntos;

            _context.MovimientosPuntos.Add(new MovimientoPuntos
            {
                TipoMovimiento = "GASTO",
                Cantidad = -puntos,
                Descripcion = $"Puntos gastados por completar la publicación: {publicacion.Titulo}",
                FechaMovimiento = DateTime.Now,
                IdUsuario = usuarioQuePaga.IdUsuario,
                IdPublicacion = publicacion.IdPublicacion
            });

            _context.MovimientosPuntos.Add(new MovimientoPuntos
            {
                TipoMovimiento = "GANANCIA",
                Cantidad = puntos,
                Descripcion = $"Puntos obtenidos por completar la publicación: {publicacion.Titulo}",
                FechaMovimiento = DateTime.Now,
                IdUsuario = usuarioQueGana.IdUsuario,
                IdPublicacion = publicacion.IdPublicacion
            });

            _context.Valoraciones.Add(valoracion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetValoracion",
                new { id = valoracion.IdValoracion },
                new
                {
                    valoracion.IdValoracion,
                    valoracion.Puntuacion,
                    valoracion.Comentario,
                    valoracion.FechaValoracion,
                    valoracion.IdPublicacion,
                    valoracion.IdUsuarioEmisor,
                    valoracion.IdUsuarioReceptor
                }
            );
        }

        // DELETE: api/Valoraciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteValoracion(int id)
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var valoracion = await _context.Valoraciones
                .FirstOrDefaultAsync(v => v.IdValoracion == id);

            if (valoracion == null)
            {
                return NotFound();
            }

            // Solo el emisor puede eliminar su valoración
            if (valoracion.IdUsuarioEmisor != usuario.IdUsuario)
            {
                return Forbid();
            }

            _context.Valoraciones.Remove(valoracion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ValoracionExists(int id)
        {
            return _context.Valoraciones.Any(e => e.IdValoracion == id);
        }
    }
}
