using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido")]
        [StringLength(254, ErrorMessage = "El email no puede exceder 254 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
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
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        public string Apellido { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido")]
        [StringLength(254, ErrorMessage = "El email no puede exceder 254 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
        public string Password { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "El formato del teléfono es inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }
        
        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string? Direccion { get; set; }
        
        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date, ErrorMessage = "Formato de fecha inválido")]
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

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido")]
        [StringLength(254, ErrorMessage = "El email no puede exceder 254 caracteres")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public UserRole Rol { get; set; }
        public UserStatus Estatus { get; set; }
        public OrganizacionInfoDto? Organizacion { get; set; }

        // Extended Profile Fields
        public string? ProfileImageUrl { get; set; }
        public string? Biografia { get; set; }
        public List<string>? Habilidades { get; set; }
        public int ExperienciaAnios { get; set; }
        public string? Disponibilidad { get; set; }
        public int HorasVoluntariado { get; set; }
        public decimal CalificacionPromedio { get; set; }
        public int TotalResenas { get; set; }
        public List<BadgeDto>? Badges { get; set; }
        public List<ResenaDto>? UltimasResenas { get; set; }
    }


    public class ResenaDto
    {
        public int Id { get; set; }
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public string UsuarioAutorNombre { get; set; } = string.Empty;
        public string OrganizacionNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
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

    // Profile DTOs
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Biografia { get; set; }
        public List<string> Habilidades { get; set; } = new List<string>();
        public int ExperienciaAnios { get; set; }
        public string? Disponibilidad { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool PerfilCompleto { get; set; }
        public int TotalResenas { get; set; }
        public double CalificacionPromedio { get; set; }
        public List<UserReviewDto> UltimasResenas { get; set; } = new List<UserReviewDto>();
        public List<BadgeDto> Badges { get; set; } = new List<BadgeDto>();
    }

    public class OrganizationProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? SitioWeb { get; set; }
        public string? TipoOrganizacion { get; set; }
        public string? NumeroRegistro { get; set; }
        public DateTime? FechaFundacion { get; set; }
        public string? Mision { get; set; }
        public string? Vision { get; set; }
        public List<string> AreasInteres { get; set; } = new List<string>();
        public string? LogoUrl { get; set; }
        public bool PerfilCompleto { get; set; }
    }

    public class UpdateUserProfileDto
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Biografia { get; set; }
        public List<string>? Habilidades { get; set; }
        public int? ExperienciaAnios { get; set; }
        public string? Disponibilidad { get; set; }
    }

    public class UpdateOrganizationProfileDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? SitioWeb { get; set; }
        public string? TipoOrganizacion { get; set; }
        public DateTime? FechaFundacion { get; set; }
        public string? Mision { get; set; }
        public string? Vision { get; set; }
        public List<string>? AreasInteres { get; set; }
    }

    public class UserReviewDto
    {
        public string OrganizacionNombre { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class ProfileCompletionDto
    {
        public int Percentage { get; set; }
        public bool IsComplete { get; set; }
        public int CompletedFields { get; set; }
        public int TotalFields { get; set; }
        public List<string> MissingFields { get; set; } = new List<string>();
    }

    public class ImageUploadResponseDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}