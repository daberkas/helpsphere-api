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

namespace API_TFCAppDavid.Controllers
{
    /// <summary>
    /// Gestión de categorias usadas para clasificar las publicaciones.
    /// La consulta está disponible para cualquier usuario, pero la creación, 
    /// modificación y eliminación de categorías queda reservada para administradores.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ASPContext _context;

        public CategoriasController(ASPContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la lista de todas las categorías disponibles.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre,
                    c.Descripcion
                })
                .ToListAsync();

            return Ok(categorias);
        }

        /// <summary>
        /// Obtiene la información de una categoría específica por su ID. 
        /// La consulta está disponible para cualquier usuario.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Where(c => c.IdCategoria == id)
                .Select(c => new
                {
                    c.IdCategoria,
                    c.Nombre,
                    c.Descripcion
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        // PUT: api/Categorias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            return BadRequest("La modificación de categorías queda reservada para administración.");
        }

        /// <summary>
        /// Crea una nueva categoría. Solo los usuarios con rol de administrador 
        /// pueden realizar esta acción.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            // Verificar que el usuario autenticado es administrador
            if (!await EsAdministrador())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return BadRequest("El nombre de la categoría es obligatorio.");
            }

            var existeCategoria = await _context.Categorias
                .AnyAsync(c => c.Nombre == categoria.Nombre);

            if (existeCategoria)
            {
                return BadRequest("Ya existe una categoría con ese nombre.");
            }

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetCategoria",
                new { id = categoria.IdCategoria },
                new
                {
                    categoria.IdCategoria,
                    categoria.Nombre,
                    categoria.Descripcion
                }
            );
        }

        /// <summary>
        /// Elimina una categoría siempre que no tenga publicaciones asociadas. 
        /// Solo los usuarios con rol de administrador pueden realizar esta acción.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            if (!await EsAdministrador())
            {
                return Forbid();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Evitar eliminar una categoría que tenga publicaciones asociadas
            var tienePublicaciones = await _context.Publicaciones
                .AnyAsync(p => p.IdCategoria == id);

            if (tienePublicaciones)
            {
                return BadRequest("No se puede eliminar una categoría con publicaciones asociadas.");
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.IdCategoria == id);
        }

        /// <summary>
        /// Obtiene el usuario autenticado a partir de las claims del token JWT. 
        /// Se busca el usuario en la base de datos utilizando el FirebaseUid extraído del token.
        /// </summary>
        private async Task<Usuario?> GetUsuarioAutenticado()
        {
            var firebaseUid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
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
        /// Comprueba si el usuario autenticado tiene el rol de administrador. 
        /// Se obtiene el usuario autenticado
        /// </summary>
        private async Task<bool> EsAdministrador()
        {
            var usuario = await GetUsuarioAutenticado();
            return usuario != null && usuario.IdRol == 1;
        }

    }
}
