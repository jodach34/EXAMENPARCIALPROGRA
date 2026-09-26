using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using EXAMENPARCIAL.Data;
using EXAMENPARCIAL.Models;

namespace EXAMENPARCIAL.Controllers
{
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "ListaIncidenciasAbiertas";

        public IncidenciasController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Incidencias
        public async Task<IActionResult> Index(string searchTerm)
        {
            // TÍTULO OBLIGATORIO Y UNIFICADO PARA LA RAMA C (TIEMPO REAL)
            ViewData["TituloPagina"] = "Incidencias abiertas en tiempo real";
            ViewData["SearchTerm"] = searchTerm;

            List<Incidencia> lista;

            // Si hay un término de búsqueda (Algolia/Búsqueda), consultamos directamente a la BD
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var query = _context.Incidencias.Where(i => i.Estado == "Abierta");
                query = query.Where(i => i.Estacion.Contains(searchTerm) || i.Descripcion.Contains(searchTerm));
                lista = await query.ToListAsync();
            }
            else
            {
                // Intentar obtener desde Redis Cache
                string cachedData = await _cache.GetStringAsync(CacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    // Caché Hit: Datos obtenidos de Redis
                    lista = JsonSerializer.Deserialize<List<Incidencia>>(cachedData) ?? new List<Incidencia>();
                }
                else
                {
                    // Caché Miss: Consultar la BD y guardar en Redis (60 segundos)
                    lista = await _context.Incidencias
                        .Where(i => i.Estado == "Abierta")
                        .ToListAsync();

                    var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                    string serializedData = JsonSerializer.Serialize(lista);
                    await _cache.SetStringAsync(CacheKey, serializedData, options);
                }
            }

            return View(lista);
        }

        // Acción para cerrar incidencia (Actualiza BD, limpia Caché y prepara evento PieHost)
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia != null)
            {
                incidencia.Estado = "Cerrada";
                await _context.SaveChangesAsync();

                // Limpiar la caché de Redis al modificar datos
                await _cache.RemoveAsync(CacheKey);

                // Evento IncidenciaActualizada listo para PieHost
            }
            return RedirectToAction(nameof(Index));
        }
    }
}