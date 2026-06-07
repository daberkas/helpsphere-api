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
    /// Gestión de valoraciones entre usuarios tras completar una colaboración.
    /// Además de registrar la valoración, se encarga de gestionar la transferencia de puntos entre el usuario que paga 
    /// y el que gana en función del tipo de publicación (solicitud u oferta) y el estado de la publicación (debe estar completada).
    /// </summary>
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

        /// <summary>
        /// Obtiene el usuario autenticado a partir del token JWT emitido por Firebase. 
        /// Se buscan diferentes claims comunes para asegurar la compatibilidad con distintas configuraciones
        /// de autenticación.
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
        /// Obtiene todas las valoraciones emitidas o recibidas por el usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Valoracion>>> GetValoraciones()
        {
            var usuario = await GetUsuarioAutenticado();

            if (usuario == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            // Mostramos únicamente las valoraciones relacionadas con el usuario actual,
            // incluyendo detalles del emisor, receptor y publicación relacionada.
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

        /// <summary>
        /// Obtiene una valoración específica por su ID, siempre que el usuario autenticado 
        /// sea el emisor o receptor de dicha valoración.
        /// </summary>
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

            // Solo el emisor o receptor de la valoración pueden verla
            if (valoracion.IdUsuarioEmisor != usuario.IdUsuario &&
                valoracion.IdUsuarioReceptor != usuario.IdUsuario)
            {
                return Forbid();
            }

            return Ok(valoracion);
        }

        /// <summary>
        /// Permite al usuario que emitió la valoración editar su puntuación y comentario, 
        /// siempre que la publicación relacionada esté completada.
        /// </summary>
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

            // Actualizamos únicamente la puntuación y el comentario,
            // no se pueden cambiar otros campos
            valoracionExistente.Puntuacion = valoracion.Puntuacion;
            valoracionExistente.Comentario = valoracion.Comentario;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Registra una nueva valoración emitida por el usuario autenticado hacia otro usuario tras completar una colaboración y
        /// ejecuta la transferencia de puntos entre ambos usuarios según el tipo de publicación y su estado.
        /// </summary>
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

            // El usuario no puede valorarse a sí mismo
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

            // Verificamos que el usuario autenticado no haya valorado ya al mismo receptor
            // en la misma publicación
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

            // Solo se pueden valorar publicaciones que estén completadas
            if (publicacion.Estado != "COMPLETADA")
            {
                return BadRequest("Solo se pueden valorar publicaciones completadas.");
            }

            var puntos = publicacion.PuntosEstimados;

            valoracion.IdUsuarioEmisor = usuario.IdUsuario;
            valoracion.FechaValoracion = DateTime.Now;

            Usuario usuarioQuePaga;
            Usuario usuarioQueGana;

            // Se determina quién paga y quién gana en función del tipo de publicación.
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

            // Verificamos que el usuario que paga tenga saldo suficiente
            // para cubrir los puntos a transferir y poder completar la operación.
            if (usuarioQuePaga.SaldoPuntos < puntos)
            {
                return BadRequest(
                    $"Saldo insuficiente. El usuario dispone de {usuarioQuePaga.SaldoPuntos} puntos y necesita {puntos}."
                );
            }

            // Se realiza la transferencia efectiva de puntos entre ambos usuarios.
            // La valoración actúa como mecanismo de cierre de la colaboración,
            // por lo que la transferencia se ejecuta en el mismo proceso para garantizar
            // la atomicidad de la operación.
            usuarioQuePaga.SaldoPuntos -= puntos;
            usuarioQueGana.SaldoPuntos += puntos;

            // Se registran los movimientos de puntos para ambos usuarios para mantener
            // la trazabilidad, indicando el motivo de la transacción.
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

        /// <summary>
        /// Permite eliminar una valoración, únicamente si el usuario autenticado 
        /// es el emisor de dicha valoración.
        /// </summary>
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

            // Solo el autor puede eliminar una valoración emitida.
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
