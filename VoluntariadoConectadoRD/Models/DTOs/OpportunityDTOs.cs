using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class OpportunityListDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int DuracionHoras { get; set; }
        public int VoluntariosRequeridos { get; set; }
        public int VoluntariosInscritos { get; set; }
        public string? AreaInteres { get; set; }
        public string? NivelExperiencia { get; set; }
        public OpportunityStatus Estatus { get; set; }
        public OrganizacionBasicDto Organizacion { get; set; } = null!;
    }

    public class OpportunityDetailDto : OpportunityListDto
    {
        public string? Requisitos { get; set; }
        public string? Beneficios { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CreateOpportunityDto
    {
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Ubicacion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required]
        [Range(1, 1000)]
        public int DuracionHoras { get; set; }

        [Required]
        [Range(1, 500)]
        public int VoluntariosRequeridos { get; set; }

        [StringLength(100)]
        public string? AreaInteres { get; set; }

        [StringLength(50)]
        public string? NivelExperiencia { get; set; }

        [StringLength(1000)]
        public string? Requisitos { get; set; }

        [StringLength(1000)]
        public string? Beneficios { get; set; }
    }

    public class UpdateOpportunityDto : CreateOpportunityDto
    {
        public OpportunityStatus? Estatus { get; set; }
    }

    public class ApplyToOpportunityDto
    {
        [StringLength(1000)]
        public string? Mensaje { get; set; }
    }

    public class OrganizacionBasicDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
    }

    public class ApplicationDto
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }
        public string OpportunityTitulo { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public ApplicationStatus Estatus { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? NotasOrganizacion { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        [Required]
        public ApplicationStatus Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class OpportunityFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? AreaInteres { get; set; }
        public string? Ubicacion { get; set; }
        public OpportunityStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}