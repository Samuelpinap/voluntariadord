using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    // Volunteer Activity DTOs
    public class VolunteerActivityDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int HorasCompletadas { get; set; }
        public ActivityStatus Estado { get; set; }
        public string? Notas { get; set; }
        public int? CalificacionVoluntario { get; set; }
        public int? CalificacionOrganizacion { get; set; }
        public string? ComentarioVoluntario { get; set; }
        public string? ComentarioOrganizacion { get; set; }
        public string OpportunityTitle { get; set; } = string.Empty;
        public string OrganizacionNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    // Skill DTOs
    public class SkillDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
    }

    public class UserSkillDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string? Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CreateUserSkillDto
    {
        [Required]
        public int SkillId { get; set; }

        [Range(1, 100)]
        public int Nivel { get; set; } = 50;
    }

    // Platform Statistics DTOs
    public class PlatformStatsDto
    {
        public int VoluntariosActivos { get; set; }
        public int OrganizacionesActivas { get; set; }
        public int ProyectosActivos { get; set; }
        public int HorasTotalesDonadas { get; set; }
        public int PersonasBeneficiadas { get; set; }
        public decimal FondosRecaudados { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    // Enhanced User Profile DTO
    public class EnhancedUserProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Biografia { get; set; }
        public int ExperienciaAnios { get; set; }
        public string? Disponibilidad { get; set; }
        public string? ImagenUrl { get; set; }
        public string? Ubicacion { get; set; }
        public bool PerfilCompleto { get; set; }
        public int TotalResenas { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public int HorasVoluntariado { get; set; }
        public int EventosParticipados { get; set; }
        public int ProyectosParticipados { get; set; }
        public UserStatus Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        
        // Collections
        public List<UserSkillDto> Habilidades { get; set; } = new List<UserSkillDto>();
        public List<BadgeDto> Badges { get; set; } = new List<BadgeDto>();
        public List<UserReviewDto> UltimasResenas { get; set; } = new List<UserReviewDto>();
        public List<VolunteerActivityDto> ActividadesRecientes { get; set; } = new List<VolunteerActivityDto>();
        public List<string> AreasInteres { get; set; } = new List<string>();
    }

    // Volunteer Statistics DTO
    public class VolunteerStatsDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int HorasVoluntariado { get; set; }
        public int EventosParticipados { get; set; }
        public int ProyectosParticipados { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public int TotalResenas { get; set; }
        public DateTime FechaRegistro { get; set; }
        public UserStatus Estado { get; set; }
        public List<VolunteerActivityDto> ActividadesRecientes { get; set; } = new List<VolunteerActivityDto>();
        public List<BadgeDto> Badges { get; set; } = new List<BadgeDto>();
        public Dictionary<string, int> EstadisticasPorMes { get; set; } = new Dictionary<string, int>();
    }

    // Application with Details DTO
    public class VolunteerApplicationDetailDto
    {
        public int Id { get; set; }
        public string OpportunityTitle { get; set; } = string.Empty;
        public string OrganizacionNombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? Ubicacion { get; set; }
        public ApplicationStatus Estado { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? Mensaje { get; set; }
        public string? NotasOrganizacion { get; set; }
        public int HorasEstimadas { get; set; }
        public int? HorasCompletadas { get; set; }
    }

    // Admin Volunteer DTO
    public class AdminVolunteerDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? ImagenUrl { get; set; }
        public UserStatus Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? OrganizacionActual { get; set; }
        public int HorasVoluntariado { get; set; }
        public int EventosParticipados { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public DateTime? UltimaActividad { get; set; }
        public string Pais { get; set; } = "República Dominicana";
    }

    // Admin Statistics DTO
    public class AdminStatsDto
    {
        public int TotalVoluntarios { get; set; }
        public int VoluntariosActivos { get; set; }
        public int VoluntariosInactivos { get; set; }
        public int VoluntariosSuspendidos { get; set; }
        public int TotalOrganizaciones { get; set; }
        public int OrganizacionesActivas { get; set; }
        public int TotalOportunidades { get; set; }
        public int OportunidadesActivas { get; set; }
        public int TotalAplicaciones { get; set; }
        public int AplicacionesPendientes { get; set; }
        public int AplicacionesAprobadas { get; set; }
        public int TotalHorasVoluntariado { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public Dictionary<string, int> VoluntariosPorMes { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AplicacionesPorMes { get; set; } = new Dictionary<string, int>();
    }

    // Paginated Result DTO
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // Admin DTOs for User Management
    public class UpdateUserStatusDto
    {
        [Required]
        public UserStatus Status { get; set; }
    }

    public class AdminEditUserDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Telefono { get; set; }
        
        public DateTime? FechaNacimiento { get; set; }
        
        public string? Biografia { get; set; }
        
        public string? Disponibilidad { get; set; }
        
        public UserStatus Status { get; set; }
    }

    // Admin DTOs for Organization Management
    public class AdminOrganizationDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? SitioWeb { get; set; }
        public string? NumeroRegistro { get; set; }
        public bool Verificada { get; set; }
        public UserStatus Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaVerificacion { get; set; }
        public string? LogoUrl { get; set; }
        public string? TipoOrganizacion { get; set; }
        public int TotalOportunidades { get; set; }
        public int OportunidadesActivas { get; set; }
        public int TotalVoluntarios { get; set; }
        public DateTime? UltimaActividad { get; set; }
    }

    public class UpdateOrganizationStatusDto
    {
        [Required]
        public UserStatus Status { get; set; }
        
        public bool Verificada { get; set; } = false;
    }

    public class AdminEditOrganizationDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        
        public string? Descripcion { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? Telefono { get; set; }
        
        public string? Direccion { get; set; }
        
        public string? SitioWeb { get; set; }
        
        public string? NumeroRegistro { get; set; }
        
        public string? TipoOrganizacion { get; set; }
        
        public UserStatus Status { get; set; }
        
        public bool Verificada { get; set; }
    }
}