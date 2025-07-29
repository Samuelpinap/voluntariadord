using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Phone]
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [StringLength(200)]
        public string? Direccion { get; set; }
        
        public DateTime FechaNacimiento { get; set; }
        
        [Required]
        public UserRole Rol { get; set; } = UserRole.Voluntario;
        
        [Required]
        public UserStatus Estatus { get; set; } = UserStatus.Activo;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime? FechaActualizacion { get; set; }

        // Navigation property
        public Organizacion? Organizacion { get; set; }

        public Usuario()
        {
        }
    }

    public enum UserRole
    {
        Voluntario = 1,
        Organizacion = 2,
        Administrador = 3
    }

    public enum UserStatus
    {
        Activo = 1,
        Inactivo = 2,
        Suspendido = 3,
        PendienteVerificacion = 4
    }
}
