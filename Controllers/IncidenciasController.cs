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
            // TÍTULO OBLIGATORIO PARA LA PREGUNTA 3 (PIEHOST)
            ViewData["TituloPagina"] = "Incidencias abiertas en tiempo real";
            ViewData["SearchTerm"] = searchTerm;

            var query = _context.Incidencias
                .Where(i => i.Estado == "Abierta");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(i => i.Estacion.Contains(searchTerm) || i.Descripcion.Contains(searchTerm));
            }

            var lista = await query.ToListAsync();
            return View(lista);
        }

        // Acción para cerrar incidencia (Simulando evento PieHost/tiempo real)
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia != null)
            {
                incidencia.Estado = "Cerrada";
                await _context.SaveChangesAsync();
                
                // Evento IncidenciaActualizada listo para PieHost
            }
            return RedirectToAction(nameof(Index));
        }
    }
}