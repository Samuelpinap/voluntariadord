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
        
        public DateTime? FechaNacimiento { get; set; }
        
        [Required]
        public UserRole Rol { get; set; } = UserRole.Voluntario;
        
        [Required]
        public UserStatus Estatus { get; set; } = UserStatus.Activo;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime? FechaActualizacion { get; set; }

        // Extended Profile Fields
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        [StringLength(1000)]
        public string? Biografia { get; set; }
        
        [StringLength(500)]
        public string? Habilidades { get; set; } // JSON string of skills
        
        public int? ExperienciaAnios { get; set; }
        
        [StringLength(50)]
        public string? Disponibilidad { get; set; } // "Matutino", "Vespertino", "Flexible"
        
        public int HorasVoluntariado { get; set; } = 0;
        
        public decimal CalificacionPromedio { get; set; } = 0;
        
        public int TotalResenas { get; set; } = 0;
        
        public bool PerfilCompleto { get; set; } = false;

        // Navigation property
        public Organizacion? Organizacion { get; set; }
        public ICollection<UsuarioResena> ResenasRecibidas { get; set; } = new List<UsuarioResena>();
        public ICollection<UsuarioBadge> Badges { get; set; } = new List<UsuarioBadge>();

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
