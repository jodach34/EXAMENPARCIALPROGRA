using EXAMENPARCIAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAMENPARCIAL.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Crear la base de datos si no existe
            context.Database.EnsureCreated();

            // Si ya hay incidencias, no hacer nada
            if (context.Incidencias.Any())
            {
                return;
            }

            context.Incidencias.AddRange(
                new Incidencia { Estacion = "Estacion Central", Descripcion = "Fallo en el sistema de anclaje de bicicletas", Prioridad = "Alta", Estado = "Abierta" },
                new Incidencia { Estacion = "Estacion Norte", Descripcion = "Cadena rota en bicicleta #402", Prioridad = "Media", Estado = "Abierta" },
                new Incidencia { Estacion = "Estacion Sur", Descripcion = "Pantalla táctil sin energía", Prioridad = "Baja", Estado = "Abierta" },
                new Incidencia { Estacion = "Estacion Oeste", Descripcion = "Neumático desinflado", Prioridad = "Media", Estado = "Cerrada" }
            );

            context.SaveChanges();
        }
    }
}