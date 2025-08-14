using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models
{
    public class UsuarioResena
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioResenadoId { get; set; } // Who is being reviewed
        
        [Required] 
        public int UsuarioAutorId { get; set; } // Who wrote the review
        
        [Required]
        public int OrganizacionId { get; set; } // Organization where the interaction happened
        
        [Range(1, 5)]
        public int Calificacion { get; set; }
        
        [StringLength(1000)]
        public string? Comentario { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Usuario UsuarioResenado { get; set; } = null!;
        public Usuario UsuarioAutor { get; set; } = null!; 
        public Organizacion Organizacion { get; set; } = null!;
    }

    public class Badge
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
        
        [StringLength(200)]
        public string? IconoUrl { get; set; }
        
        [StringLength(20)]
        public string Color { get; set; } = "primary"; // Bootstrap color classes
        
        [StringLength(50)]
        public string? Categoria { get; set; }
        
        public BadgeType Tipo { get; set; }
        
        public int RequisitoValor { get; set; } = 0; // Hours, years, count, etc.

        public bool EsActivo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<UsuarioBadge> UsuarioBadges { get; set; } = new List<UsuarioBadge>();
    }

    public class UsuarioBadge
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        public int BadgeId { get; set; }
        
        public DateTime FechaObtenido { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? NotasObtencion { get; set; }

        // Navigation properties
        public Usuario Usuario { get; set; } = null!;
        public Badge Badge { get; set; } = null!;
    }

    public enum BadgeType
    {
        Tiempo = 1,        // Hours-based
        Experiencia = 2,   // Years-based  
        Actividad = 3,     // Activity count
        Calificacion = 4,  // Rating-based
        Especial = 5       // Special achievements
    }
}