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
    /// Consulta de roles disponibles en el sistema. 
    /// Los roles son gestionados internamente y no pueden ser creados, 
    /// modificados o eliminados desde el cliente.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ASPContext _context;

        public RolesController(ASPContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la lista de roles disponibles en el sistema.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new
                {
                    r.IdRol,
                    r.NombreRol
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Obtiene la información de un rol concreto por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetRol(int id)
        {
            var rol = await _context.Roles
                .Where(r => r.IdRol == id)
                .Select(r => new
                {
                    r.IdRol,
                    r.NombreRol
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound();
            }

            return Ok(rol);
        }

        /// <summary>
        /// Operación no permitida. Los roles no pueden modificarse desde el cliente. 
        /// La modificación de roles queda reservada para administración.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(int id, Rol rol)
        {
            return BadRequest("La modificación de roles queda reservada para administración.");
        }

        /// <summary>
        /// Operación no permitida. Los roles se gestionan internamente y no pueden ser creados
        /// desde el cliente.
        /// </summary>
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Rol>> PostRol(Rol rol)
        {
            return BadRequest("La creación de roles queda reservada para administración.");
        }

        /// <summary>
        /// Operación no permitida. Los roles se gestionan internamente y no pueden ser eliminados 
        /// desde el cliente.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            return BadRequest("La eliminación de roles queda reservada para administración.");
        }

        private bool RolExists(int id)
        {
            return _context.Roles.Any(e => e.IdRol == id);
        }
    }
}
