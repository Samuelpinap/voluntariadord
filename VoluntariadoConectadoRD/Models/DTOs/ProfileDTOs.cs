using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    // User Profile DTOs
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Avatar { get; set; }
        public string? Biografia { get; set; }
        public string? Intereses { get; set; }
        public string? Habilidades { get; set; }
        public string? Disponibilidad { get; set; }
        public string? ExperienciaPrevia { get; set; }
        public string? Ubicacion { get; set; }
        public bool PerfilCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public class UpdateUserProfileDto
    {
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [StringLength(200)]
        public string? Direccion { get; set; }
        
        [StringLength(1000)]
        public string? Biografia { get; set; }
        
        [StringLength(2000)]
        public string? Intereses { get; set; }
        
        [StringLength(1000)]
        public string? Habilidades { get; set; }
        
        [StringLength(500)]
        public string? Disponibilidad { get; set; }
        
        [StringLength(1000)]
        public string? ExperienciaPrevia { get; set; }
        
        [StringLength(200)]
        public string? Ubicacion { get; set; }
    }

    // Organization Profile DTOs
    public class OrganizationProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? SitioWeb { get; set; }
        public string? NumeroRegistro { get; set; }
        public string? Logo { get; set; }
        public string? Mision { get; set; }
        public string? Vision { get; set; }
        public string? AreasEnfoque { get; set; }
        public string? PersonaContacto { get; set; }
        public string? CargoContacto { get; set; }
        public string? TelefonoContacto { get; set; }
        public bool PerfilCompleto { get; set; }
        public OrganizacionStatus Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public UserInfoDto? UsuarioAdministrador { get; set; }
    }

    public class UpdateOrganizationProfileDto
    {
        [StringLength(1000)]
        public string? Descripcion { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [StringLength(300)]
        public string? Direccion { get; set; }
        
        [Url]
        [StringLength(200)]
        public string? SitioWeb { get; set; }
        
        [StringLength(50)]
        public string? NumeroRegistro { get; set; }
        
        [StringLength(1000)]
        public string? Mision { get; set; }
        
        [StringLength(1000)]
        public string? Vision { get; set; }
        
        [StringLength(1000)]
        public string? AreasEnfoque { get; set; }
        
        [StringLength(200)]
        public string? PersonaContacto { get; set; }
        
        [StringLength(100)]
        public string? CargoContacto { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? TelefonoContacto { get; set; }
    }

    // Profile Completion DTOs
    public class ProfileCompletionDto
    {
        public bool IsComplete { get; set; }
        public int CompletionPercentage { get; set; }
        public List<string> MissingFields { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
    }

    // Image Upload DTOs
    public class ImageUploadDto
    {
        [Required]
        public IFormFile Image { get; set; } = null!;
        
        public string? Description { get; set; }
    }

    public class ImageResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
    }
}