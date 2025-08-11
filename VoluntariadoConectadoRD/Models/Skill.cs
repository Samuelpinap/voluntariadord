using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models
{
    public class Skill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(100)]
        public string? Categoria { get; set; }

        [StringLength(200)]
        public string? IconoUrl { get; set; }

        [StringLength(20)]
        public string Color { get; set; } = "primary";

        public bool EsActivo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<UsuarioSkill> UsuarioSkills { get; set; } = new List<UsuarioSkill>();
    }

    public class UsuarioSkill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int SkillId { get; set; }

        [Range(1, 100)]
        public int Nivel { get; set; } = 50; // Nivel de competencia del 1-100

        [StringLength(200)]
        public string? Certificacion { get; set; }

        public DateTime? FechaAdquisicion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Skill Skill { get; set; } = null!;
    }
}