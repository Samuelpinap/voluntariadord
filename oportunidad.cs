// Models/Oportunidad.cs
using System.ComponentModel.DataAnnotations;

namespace VoluntariadoApi.Models
{
    public class Oportunidad
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required]
        [StringLength(150)]
        public string Ubicacion { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        
        [Required]
        public DateTime FechaInicio { get; set; }
        
        [Required]
        public DateTime FechaFin { get; set; }
    }
}

// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using VoluntariadoApi.Models;

namespace VoluntariadoApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Oportunidad> Oportunidades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones adicionales si son necesarias
            modelBuilder.Entity<Oportunidad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Ubicacion).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaInicio).IsRequired();
                entity.Property(e => e.FechaFin).IsRequired();
            });
        }
    }
}

// Controllers/OportunidadesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoluntariadoApi.Data;
using VoluntariadoApi.Models;

namespace VoluntariadoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OportunidadesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OportunidadesController> _logger;

        public OportunidadesController(ApplicationDbContext context, ILogger<OportunidadesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las oportunidades de voluntariado
        /// </summary>
        /// <returns>Lista de oportunidades de voluntariado</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Oportunidad>>> GetOportunidades()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las oportunidades de voluntariado");

                var oportunidades = await _context.Oportunidades
                    .OrderBy(o => o.FechaInicio)
                    .ToListAsync();

                _logger.LogInformation($"Se encontraron {oportunidades.Count} oportunidades");

                return Ok(oportunidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las oportunidades de voluntariado");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}

// Program.cs (configuración necesaria)
using Microsoft.EntityFrameworkCore;
using VoluntariadoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

// appsettings.json (ejemplo de configuración)
/*
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VoluntariadoDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
*/
