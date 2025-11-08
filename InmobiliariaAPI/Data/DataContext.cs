using InmobiliariaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Persona - PersonaRole - Role: Relación muchos a muchos
            modelBuilder.Entity<PersonaRole>()
                .HasKey(pr => new { pr.PersonaId, pr.RolId });
        }


        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<PersonaRole> PersonaRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<TipoInmueble> TipoInmuebles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


    }
}
