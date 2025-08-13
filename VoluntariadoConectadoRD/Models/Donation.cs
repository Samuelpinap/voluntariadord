using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        // Optional link to financial report (for legacy compatibility)
        public int? FinancialReportId { get; set; }

        [ForeignKey("FinancialReportId")]
        public virtual FinancialReport? FinancialReport { get; set; }

        // Direct link to organization for PayPal donations
        public int? OrganizacionId { get; set; }

        [ForeignKey("OrganizacionId")]
        public virtual Organizacion? Organizacion { get; set; }

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

        // PayPal-specific fields
        [StringLength(100)]
        public string? PayPalTransactionId { get; set; }

        [StringLength(100)]
        public string? PayPalOrderId { get; set; }

        [StringLength(50)]
        public string? PayPalPaymentStatus { get; set; }

        [StringLength(150)]
        public string? PayPalPayerEmail { get; set; }

        [StringLength(200)]
        public string? PayPalPayerId { get; set; }

        // Payment method enum
        public DonationPaymentMethod MetodoPago { get; set; } = DonationPaymentMethod.Manual;

        // Payment status
        public DonationStatus EstadoPago { get; set; } = DonationStatus.Pendiente;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }

    public enum DonationPaymentMethod
    {
        Manual = 1,
        PayPal = 2,
        Transferencia = 3,
        Efectivo = 4
    }

    public enum DonationStatus
    {
        Pendiente = 1,
        Completado = 2,
        Fallido = 3,
        Cancelado = 4,
        Reembolsado = 5
    }
}