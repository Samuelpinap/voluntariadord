using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Data
{
    public class DbContextApplication : DbContext
    {
        public DbContextApplication(DbContextOptions<DbContextApplication> opts) : base(opts) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<VolunteerOpportunity> VolunteerOpportunities { get; set; }
        public DbSet<VolunteerApplication> VolunteerApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
            });

            // Configuración para Organizacion
            modelBuilder.Entity<Organizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(300);
                entity.Property(e => e.SitioWeb).HasMaxLength(200);
                entity.Property(e => e.NumeroRegistro).HasMaxLength(50);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithOne(u => u.Organizacion)
                      .HasForeignKey<Organizacion>(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Roles (legacy)
            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("role");
                entity.HasKey(e => e.id);
            });

            // Configuración para VolunteerOpportunity
            modelBuilder.Entity<VolunteerOpportunity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Ubicacion).HasMaxLength(500);
                entity.Property(e => e.AreaInteres).HasMaxLength(100);
                entity.Property(e => e.NivelExperiencia).HasMaxLength(50);
                entity.Property(e => e.Requisitos).HasMaxLength(1000);
                entity.Property(e => e.Beneficios).HasMaxLength(1000);

                // Relación con Organizacion
                entity.HasOne(e => e.Organizacion)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizacionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para VolunteerApplication
            modelBuilder.Entity<VolunteerApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Mensaje).HasMaxLength(1000);
                entity.Property(e => e.NotasOrganizacion).HasMaxLength(500);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación con VolunteerOpportunity
                entity.HasOne(e => e.Opportunity)
                      .WithMany(o => o.Aplicaciones)
                      .HasForeignKey(e => e.OpportunityId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice único para evitar aplicaciones duplicadas
                entity.HasIndex(e => new { e.UsuarioId, e.OpportunityId }).IsUnique();
            });

            // Mapeo de tablas para compatibilidad
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Organizacion>().ToTable("organizaciones");
            modelBuilder.Entity<VolunteerOpportunity>().ToTable("volunteer_opportunities");
            modelBuilder.Entity<VolunteerApplication>().ToTable("volunteer_applications");
        }
    }
}
