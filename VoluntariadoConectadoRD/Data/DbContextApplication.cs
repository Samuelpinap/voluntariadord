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
        public DbSet<UsuarioResena> UsuarioResenas { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UsuarioBadge> UsuarioBadges { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UsuarioSkill> UsuarioSkills { get; set; }
        public DbSet<VolunteerActivity> VolunteerActivities { get; set; }
        public DbSet<PlatformStats> PlatformStats { get; set; }

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
                entity.Property(e => e.CalificacionPromedio).HasPrecision(3, 2);
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

            // Configuración para UsuarioResena
            modelBuilder.Entity<UsuarioResena>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comentario).HasMaxLength(1000);

                // Relación con Usuario Reseñado
                entity.HasOne(e => e.UsuarioResenado)
                      .WithMany(u => u.ResenasRecibidas)
                      .HasForeignKey(e => e.UsuarioResenadoId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación con Usuario Autor
                entity.HasOne(e => e.UsuarioAutor)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioAutorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación con Organizacion
                entity.HasOne(e => e.Organizacion)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizacionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Badge
            modelBuilder.Entity<Badge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.IconoUrl).HasMaxLength(200);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.Categoria).HasMaxLength(50);
            });

            // Configuración para UsuarioBadge
            modelBuilder.Entity<UsuarioBadge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NotasObtencion).HasMaxLength(500);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithMany(u => u.Badges)
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Badge
                entity.HasOne(e => e.Badge)
                      .WithMany(b => b.UsuarioBadges)
                      .HasForeignKey(e => e.BadgeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice único para evitar badges duplicados
                entity.HasIndex(e => new { e.UsuarioId, e.BadgeId }).IsUnique();
            });

            // Configuración para Skill
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Categoria).HasMaxLength(100);
            });

            // Configuración para UsuarioSkill
            modelBuilder.Entity<UsuarioSkill>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Skill
                entity.HasOne(e => e.Skill)
                      .WithMany(s => s.UsuarioSkills)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice único para evitar skills duplicadas por usuario
                entity.HasIndex(e => new { e.UsuarioId, e.SkillId }).IsUnique();
            });

            // Configuración para VolunteerActivity
            modelBuilder.Entity<VolunteerActivity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.Notas).HasMaxLength(500);
                entity.Property(e => e.ComentarioVoluntario).HasMaxLength(1000);
                entity.Property(e => e.ComentarioOrganizacion).HasMaxLength(1000);

                // Relación con Usuario
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación con VolunteerOpportunity
                entity.HasOne(e => e.Opportunity)
                      .WithMany()
                      .HasForeignKey(e => e.OpportunityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para PlatformStats
            modelBuilder.Entity<PlatformStats>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FondosRecaudados).HasPrecision(18, 2);
                entity.Property(e => e.NotasEstadisticas).HasMaxLength(500);
            });

            // Mapeo de tablas para compatibilidad
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Organizacion>().ToTable("organizaciones");
            modelBuilder.Entity<VolunteerOpportunity>().ToTable("volunteer_opportunities");
            modelBuilder.Entity<VolunteerApplication>().ToTable("volunteer_applications");
            modelBuilder.Entity<UsuarioResena>().ToTable("usuario_resenas");
            modelBuilder.Entity<Badge>().ToTable("badges");
            modelBuilder.Entity<UsuarioBadge>().ToTable("usuario_badges");
            modelBuilder.Entity<Skill>().ToTable("skills");
            modelBuilder.Entity<UsuarioSkill>().ToTable("usuario_skills");
            modelBuilder.Entity<VolunteerActivity>().ToTable("volunteer_activities");
            modelBuilder.Entity<PlatformStats>().ToTable("platform_stats");
        }
    }
}
