using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class PayPalTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PayPalOrderId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PayPalTransactionId { get; set; }

        [Required]
        public int OrganizacionId { get; set; }

        [ForeignKey("OrganizacionId")]
        public virtual Organizacion Organizacion { get; set; } = null!;

        public int? DonationId { get; set; }

        [ForeignKey("DonationId")]
        public virtual Donation? Donation { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(150)]
        public string? PayerEmail { get; set; }

        [StringLength(200)]
        public string? PayerId { get; set; }

        [StringLength(200)]
        public string? PayerName { get; set; }

        [Column(TypeName = "TEXT")]
        public string? RawPayPalResponse { get; set; }

        [Column(TypeName = "TEXT")]
        public string? WebhookData { get; set; }

        public PayPalTransactionType TransactionType { get; set; } = PayPalTransactionType.Payment;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public enum PayPalTransactionType
    {
        Payment = 1,
        Refund = 2,
        Dispute = 3,
        Chargeback = 4
    }
}