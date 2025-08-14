using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class VolunteerOpportunity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(2500)]
        public string? Ubicacion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required]
        public int DuracionHoras { get; set; }

        [Required]
        public int VoluntariosRequeridos { get; set; }

        public int VoluntariosInscritos { get; set; } = 0;

        [StringLength(100)]
        public string? AreaInteres { get; set; }

        [StringLength(50)]
        public string? NivelExperiencia { get; set; }

        [StringLength(1000)]
        public string? Requisitos { get; set; }

        [StringLength(1000)]
        public string? Beneficios { get; set; }

        [Required]
        public OpportunityStatus Estatus { get; set; } = OpportunityStatus.Activa;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Compatibility properties
        public int Estado => (int)Estatus;
        public string? Categoria => AreaInteres;

        // Foreign Key to Organization
        [Required]
        public int OrganizacionId { get; set; }

        [ForeignKey("OrganizacionId")]
        public virtual Organizacion Organizacion { get; set; } = null!;

        // Navigation property for applications
        public virtual ICollection<VolunteerApplication> Aplicaciones { get; set; } = new List<VolunteerApplication>();
        public virtual ICollection<VolunteerApplication> VolunteerApplications { get; set; } = new List<VolunteerApplication>(); // Alias for compatibility
    }

    public enum OpportunityStatus
    {
        Borrador = 0,
        Activa = 1,
        Pausada = 2,
        Cerrada = 3,
        Completada = 4
    }

    public class VolunteerApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        // Alias for compatibility
        public int VolunteerId => UsuarioId;

        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        [Required]
        public int OpportunityId { get; set; }

        [ForeignKey("OpportunityId")]
        public virtual VolunteerOpportunity Opportunity { get; set; } = null!;

        [StringLength(1000)]
        public string? Mensaje { get; set; }

        [Required]
        public ApplicationStatus Estatus { get; set; } = ApplicationStatus.Pendiente;

        [Required]
        public DateTime FechaAplicacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaRespuesta { get; set; }

        [StringLength(500)]
        public string? NotasOrganizacion { get; set; }

        // Compatibility property
        public int Estado => (int)Estatus;
    }

    public enum ApplicationStatus
    {
        Pendiente = 0,
        Aceptada = 1,
        Rechazada = 2,
        Retirada = 3,
        Completado = 4
    }
}