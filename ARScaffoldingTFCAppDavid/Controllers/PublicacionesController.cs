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
    /// Gestión de publicaciones que pueden ser solicitudes u ofertas de servicios.
    /// Permite crear, consultar, modificar y eliminar publicaciones.
    /// Solo el creador de la publicación puede modificarla o eliminarla, respetando la logica de negocio 
    /// de la plataforma. Las publicaciones con actividad asociada no pueden ser eliminadas, solo canceladas.
    /// </summary>
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

        /// <summary>
        /// Obtiene el usuario autenticado a partir del token JWT, buscando su Firebase UID 
        /// y luego su registro en la base de datos.
        /// </summary>
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

        /// <summary>
        /// Obtiene todas las publicaciones disponibles. 
        /// Se pueden aplicar filtros por tipo, estado, zona o categoría.
        /// </summary>
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

        /// <summary>
        /// Obtiene una publicación específica por su ID. 
        /// Devuelve detalles de la publicación.
        /// </summary>
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

        /// <summary>
        /// Permite actualizar una publicación existente o 
        /// modificar solo su estado según los permisos del usuario autenticado.
        /// </summary>
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

            // Comprobamos si el usuario es el creador de la publicación
            var esCreador = publicacionExistente.IdUsuarioCreador == usuario.IdUsuario;

            // Comprobamos si el usuario participa en la publicación y fue aceptado
            // para cambiar solo el estado a COMPLETADA o CANCELADA
            var esUsuarioAceptado = await _context.SolicitudesParticipacion
                .AnyAsync(s =>
                    s.IdPublicacion == id &&
                    s.IdUsuarioSolicitante == usuario.IdUsuario &&
                    s.Estado == "ACEPTADA");

            var nuevoEstado = publicacion.Estado?.ToUpper();

            var cambioSoloEstadoFinal =
                nuevoEstado == "COMPLETADA" ||
                nuevoEstado == "CANCELADA";

            //Solo el creador puede modificar todos los campos, pero un usuario aceptado
            //solo puede cambiar el estado a COMPLETADA o CANCELADA
            if (!esCreador && !(esUsuarioAceptado && cambioSoloEstadoFinal))
            {
                return Forbid();
            }

            // Se aplican cambios según el rol del usuario respecto a la publicación: Si el usuario
            // no es el creador, solo se permite cambiar el estado a COMPLETADA o CANCELADA.
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

        /// <summary>
        /// Crea una nueva publicación de tipo solicitud u oferta.
        /// </summary>
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

            // Validación del tipo publicación permitido. Solo se permiten "SOLICITUD" u "OFERTA"
            if (publicacion.TipoPublicacion != "SOLICITUD" && publicacion.TipoPublicacion != "OFERTA")
            {
                return BadRequest("El tipo de publicación debe ser SOLICITUD u OFERTA.");
            }

            // Validación de la cantidad de puntos solicitados o ofrecidos.
            // Debe ser un número positivo mayor que cero.
            if (publicacion.PuntosEstimados <= 0)
            {
                return BadRequest("Los puntos estimados deben ser mayores que cero.");
            }

            // Verificación de saldo de puntos para publicaciones de tipo SOLICITUD.
            // El usuario debe tener saldo suficiente para crear la solicitud.
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

        /// <summary>
        /// Elimina una publicación existente. Solo el creador de la publicación puede eliminarla, 
        /// y solo si no tiene actividad asociada.
        /// </summary>
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

            // Se evita el borrado físico de publicaciones con actividad registrada,
            // para mantener la integridad referencial de la información y el historial
            // de transacciones.

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
