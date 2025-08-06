using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class FinancialReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrganizacionId { get; set; }

        [ForeignKey("OrganizacionId")]
        public virtual Organizacion Organizacion { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public int Año { get; set; }

        [Required]
        [Range(1, 4)]
        public int Trimestre { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIngresos { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGastos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [StringLength(1000)]
        public string? Resumen { get; set; }

        [StringLength(500)]
        public string? DocumentoUrl { get; set; }

        public bool EsPublico { get; set; } = true;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        public virtual ICollection<Expense> Gastos { get; set; } = new List<Expense>();
        public virtual ICollection<Donation> Donaciones { get; set; } = new List<Donation>();
    }
}