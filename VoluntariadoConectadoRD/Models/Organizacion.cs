using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models
{
    public class Organizacion
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Descripcion { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
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
        
        [Required]
        public OrganizacionStatus Estatus { get; set; } = OrganizacionStatus.PendienteVerificacion;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime? FechaActualizacion { get; set; }
        
        public DateTime? FechaVerificacion { get; set; }
        
        // Extended Organization Profile Fields
        [StringLength(100)]
        public string? TipoOrganizacion { get; set; }
        
        public DateTime? FechaFundacion { get; set; }
        
        [StringLength(1000)]
        public string? Mision { get; set; }
        
        [StringLength(1000)]
        public string? Vision { get; set; }
        
        [StringLength(500)]
        public string? AreasInteres { get; set; } // JSON string of interest areas
        
        [StringLength(500)]
        public string? LogoUrl { get; set; }
        
        public bool PerfilCompleto { get; set; } = false;
        
        public bool Verificada { get; set; } = false;
        
        // Usuario administrador de la organización
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<VolunteerOpportunity> Opportunities { get; set; } = new List<VolunteerOpportunity>();

        public Organizacion()
        {
        }
    }

    public enum OrganizacionStatus
    {
        Activa = 1,
        Inactiva = 2,
        Suspendida = 3,
        PendienteVerificacion = 4,
        Rechazada = 5
    }
}