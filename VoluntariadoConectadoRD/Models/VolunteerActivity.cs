using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class VolunteerActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int OpportunityId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Range(0, 24)]
        public int HorasCompletadas { get; set; } = 0;

        [Required]
        public ActivityStatus Estado { get; set; } = ActivityStatus.Programada;

        [StringLength(500)]
        public string? Notas { get; set; }

        [Range(1, 5)]
        public int? CalificacionVoluntario { get; set; }

        [Range(1, 5)]
        public int? CalificacionOrganizacion { get; set; }

        [StringLength(1000)]
        public string? ComentarioVoluntario { get; set; }

        [StringLength(1000)]
        public string? ComentarioOrganizacion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Navigation properties
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("OpportunityId")]
        public virtual VolunteerOpportunity Opportunity { get; set; } = null!;
    }

    public enum ActivityStatus
    {
        Programada = 0,
        EnProgreso = 1,
        Completada = 2,
        Cancelada = 3,
        NoCompletada = 4
    }

    public class PlatformStats
    {
        [Key]
        public int Id { get; set; }

        public int VoluntariosActivos { get; set; } = 0;

        public int OrganizacionesActivas { get; set; } = 0;

        public int ProyectosActivos { get; set; } = 0;

        public int HorasTotalesDonadas { get; set; } = 0;

        public int PersonasBeneficiadas { get; set; } = 0;

        public decimal FondosRecaudados { get; set; } = 0;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? NotasEstadisticas { get; set; }
    }
}