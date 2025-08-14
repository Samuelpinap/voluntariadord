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
        public DbSet<FinancialReport> FinancialReports { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserOnlineStatus> UserOnlineStatuses { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<PayPalTransaction> PayPalTransactions { get; set; }

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
                entity.Property(e => e.SaldoActual).HasPrecision(18, 2);

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
                      .WithMany(o => o.Opportunities)
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

            // Configuración para FinancialReport
            modelBuilder.Entity<FinancialReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TotalIngresos).HasPrecision(18, 2);
                entity.Property(e => e.TotalGastos).HasPrecision(18, 2);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.Property(e => e.Resumen).HasMaxLength(1000);
                entity.Property(e => e.DocumentoUrl).HasMaxLength(500);

                // Relación con Organizacion
                entity.HasOne<Organizacion>(e => e.Organizacion)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizacionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Expense
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Monto).HasPrecision(18, 2);
                entity.Property(e => e.Justificacion).HasMaxLength(500);
                entity.Property(e => e.DocumentoUrl).HasMaxLength(500);

                // Relación con FinancialReport
                entity.HasOne(e => e.FinancialReport)
                      .WithMany(fr => fr.Gastos)
                      .HasForeignKey(e => e.FinancialReportId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración para Donation
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Donante).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Monto).HasPrecision(18, 2);
                entity.Property(e => e.Proposito).HasMaxLength(500);
                entity.Property(e => e.DocumentoUrl).HasMaxLength(500);
                entity.Property(e => e.PayPalTransactionId).HasMaxLength(100);
                entity.Property(e => e.PayPalOrderId).HasMaxLength(100);
                entity.Property(e => e.PayPalPaymentStatus).HasMaxLength(50);
                entity.Property(e => e.PayPalPayerEmail).HasMaxLength(150);
                entity.Property(e => e.PayPalPayerId).HasMaxLength(200);

                // Relación con FinancialReport (opcional)
                entity.HasOne(e => e.FinancialReport)
                      .WithMany(fr => fr.Donaciones)
                      .HasForeignKey(e => e.FinancialReportId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Organizacion (opcional, para donaciones directas)
                entity.HasOne(e => e.Organizacion)
                      .WithMany(o => o.Donations)
                      .HasForeignKey(e => e.OrganizacionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para PayPal
                entity.HasIndex(e => e.PayPalTransactionId);
                entity.HasIndex(e => e.PayPalOrderId);
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
            modelBuilder.Entity<FinancialReport>().ToTable("financial_reports");
            modelBuilder.Entity<Expense>().ToTable("expenses");
            modelBuilder.Entity<Donation>().ToTable("donations");
            // Configuración para Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ActionUrl).HasMaxLength(200);
                entity.Property(e => e.Data).HasColumnType("TEXT");

                // Relación con Usuario (Recipient)
                entity.HasOne(e => e.Recipient)
                      .WithMany()
                      .HasForeignKey(e => e.RecipientId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Usuario (Sender) - Opcional
                entity.HasOne(e => e.Sender)
                      .WithMany()
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índices para mejorar performance
                entity.HasIndex(e => e.RecipientId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configuración para UserOnlineStatus
            modelBuilder.Entity<UserOnlineStatus>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.ConnectionId).HasMaxLength(100);

                // Relación con Usuario
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<UserOnlineStatus>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.IsOnline);
                entity.HasIndex(e => e.LastSeen);
            });

            // Configuración para Message
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ConversationId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AttachmentFileName).HasMaxLength(255);
                entity.Property(e => e.AttachmentMimeType).HasMaxLength(100);
                entity.Property(e => e.AttachmentUrl).HasMaxLength(500);

                // Relaciones
                entity.HasOne(e => e.Sender)
                      .WithMany()
                      .HasForeignKey(e => e.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Recipient)
                      .WithMany()
                      .HasForeignKey(e => e.RecipientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReplyToMessage)
                      .WithMany(e => e.Replies)
                      .HasForeignKey(e => e.ReplyToMessageId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índices para performance
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.RecipientId);
                entity.HasIndex(e => e.ConversationId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.IsDeleted);
                entity.HasIndex(e => new { e.RecipientId, e.IsRead, e.IsDeleted });
            });

            // Configuración para Conversation
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);

                // Relaciones
                entity.HasOne(e => e.User1)
                      .WithMany()
                      .HasForeignKey(e => e.User1Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User2)
                      .WithMany()
                      .HasForeignKey(e => e.User2Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastMessage)
                      .WithMany()
                      .HasForeignKey(e => e.LastMessageId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Relación con Messages
                entity.HasMany(e => e.Messages)
                      .WithOne()
                      .HasForeignKey(m => m.ConversationId)
                      .HasPrincipalKey(c => c.Id)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índices
                entity.HasIndex(e => e.User1Id);
                entity.HasIndex(e => e.User2Id);
                entity.HasIndex(e => e.LastMessageAt);
                entity.HasIndex(e => e.IsArchived);
                entity.HasIndex(e => new { e.User1Id, e.User1HasUnread });
                entity.HasIndex(e => new { e.User2Id, e.User2HasUnread });
            });

            // Configuración para PayPalTransaction
            modelBuilder.Entity<PayPalTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PayPalOrderId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PayPalTransactionId).HasMaxLength(100);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PayerEmail).HasMaxLength(150);
                entity.Property(e => e.PayerId).HasMaxLength(200);
                entity.Property(e => e.PayerName).HasMaxLength(200);
                entity.Property(e => e.RawPayPalResponse).HasColumnType("TEXT");
                entity.Property(e => e.WebhookData).HasColumnType("TEXT");

                // Relación con Organizacion
                entity.HasOne(e => e.Organizacion)
                      .WithMany()
                      .HasForeignKey(e => e.OrganizacionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación con Donation (opcional)
                entity.HasOne(e => e.Donation)
                      .WithMany()
                      .HasForeignKey(e => e.DonationId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índices
                entity.HasIndex(e => e.PayPalOrderId).IsUnique();
                entity.HasIndex(e => e.PayPalTransactionId);
                entity.HasIndex(e => e.OrganizacionId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Tabla mapping
            modelBuilder.Entity<PayPalTransaction>().ToTable("paypal_transactions");
        }
    }
}
