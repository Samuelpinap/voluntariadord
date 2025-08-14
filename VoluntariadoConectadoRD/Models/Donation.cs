using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FinancialReportId { get; set; }

        [ForeignKey("FinancialReportId")]
        public virtual FinancialReport FinancialReport { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Donante { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [StringLength(500)]
        public string? Proposito { get; set; }

        [StringLength(500)]
        public string? DocumentoUrl { get; set; }

        public bool EsRecurrente { get; set; } = false;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}