using Microsoft.EntityFrameworkCore;
using EXAMENPARCIAL.Models;

namespace EXAMENPARCIAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Incidencia> Incidencias { get; set; }
    }
}