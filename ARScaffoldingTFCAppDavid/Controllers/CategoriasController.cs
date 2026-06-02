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

        // GET: api/Categorias
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

        // GET: api/Categorias/5
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

        // POST: api/Categorias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
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

        // DELETE: api/Categorias/5
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

        private async Task<bool> EsAdministrador()
        {
            var usuario = await GetUsuarioAutenticado();
            return usuario != null && usuario.IdRol == 1;
        }

    }
}
