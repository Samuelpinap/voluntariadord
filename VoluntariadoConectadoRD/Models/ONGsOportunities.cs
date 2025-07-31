using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models
{
    public class Oportunidad
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Descripcion { get; set; } = string.Empty;
        
        [Required]
        [StringLength(150)]
        public string Ubicacion { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        
        [Required]
        public DateTime FechaInicio { get; set; }
        
        [Required]
        public DateTime FechaFin { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
            {
                yield return new ValidationResult(
                    "FechaFin must be greater than or equal to FechaInicio.",
                    new[] { nameof(FechaFin), nameof(FechaInicio) }
                );
            }
        }
    }
}
