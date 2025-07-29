using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    public class RegisterVoluntarioDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Phone]
        public string? Telefono { get; set; }
        
        public string? Direccion { get; set; }
        
        [Required]
        public DateTime FechaNacimiento { get; set; }
    }

    public class RegisterOrganizacionDto
    {
        [Required]
        [StringLength(200)]
        public string NombreOrganizacion { get; set; } = string.Empty;
        
        public string? DescripcionOrganizacion { get; set; }
        
        [Required]
        [EmailAddress]
        public string EmailOrganizacion { get; set; } = string.Empty;
        
        [Phone]
        public string? TelefonoOrganizacion { get; set; }
        
        public string? DireccionOrganizacion { get; set; }
        
        [Url]
        public string? SitioWeb { get; set; }
        
        public string? NumeroRegistro { get; set; }
        
        // Datos del usuario administrador
        [Required]
        [StringLength(100)]
        public string NombreAdmin { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ApellidoAdmin { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string EmailAdmin { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string PasswordAdmin { get; set; } = string.Empty;
        
        [Phone]
        public string? TelefonoAdmin { get; set; }
        
        [Required]
        public DateTime FechaNacimientoAdmin { get; set; }
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public UserRole Rol { get; set; }
        public UserStatus Estatus { get; set; }
        public OrganizacionInfoDto? Organizacion { get; set; }
    }

    public class OrganizacionInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Email { get; set; } = string.Empty;
        public OrganizacionStatus Estatus { get; set; }
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
    }
}