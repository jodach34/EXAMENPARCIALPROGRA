using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAMENPARCIAL.Data;
using EXAMENPARCIAL.Models;

namespace EXAMENPARCIAL.Controllers
{
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncidenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Incidencias
        public async Task<IActionResult> Index(string searchTerm)
        {
            // TÍTULO COMPARTIDO OBLIGATORIO PARA LA PREGUNTA 1
            ViewData["TituloPagina"] = "Incidencias abiertas encontradas";
            ViewData["SearchTerm"] = searchTerm;

            var query = _context.Incidencias
                .Where(i => i.Estado == "Abierta");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Búsqueda por estación o descripción
                query = query.Where(i => i.Estacion.Contains(searchTerm) || i.Descripcion.Contains(searchTerm));
            }

            var lista = await query.ToListAsync();
            return View(lista);
        }

        // Acción para cerrar incidencia
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia != null)
            {
                incidencia.Estado = "Cerrada";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}