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
            // TÍTULO OBLIGATORIO PARA LA PREGUNTA 2 (REDIS)
            ViewData["TituloPagina"] = "Incidencias abiertas con consulta rápida";
            ViewData["SearchTerm"] = searchTerm;

            List<Incidencia> lista;

            // Si hay un término de búsqueda, consultamos directamente a la BD
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
                    // Caché Miss: Consultar la BD y guardar en Redis
                    lista = await _context.Incidencias
                        .Where(i => i.Estado == "Abierta")
                        .ToListAsync();

                    var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)); // 60 segundos como pide el examen

                    string serializedData = JsonSerializer.Serialize(lista);
                    await _cache.SetStringAsync(CacheKey, serializedData, options);
                }
            }

            return View(lista);
        }

        // Acción para cerrar incidencia (y limpiar la caché para reflejar el cambio)
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}