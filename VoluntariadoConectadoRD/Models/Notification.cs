using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RecipientId { get; set; }

        public int? SenderId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // application, approval, rejection, message, system, etc.

        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Additional data as JSON string
        public string? Data { get; set; }

        // Priority level
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        // Navigation properties
        [ForeignKey("RecipientId")]
        public virtual Usuario Recipient { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual Usuario? Sender { get; set; }
    }

    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    public class UserOnlineStatus
    {
        [Key]
        public int UserId { get; set; }

        public bool IsOnline { get; set; } = false;

        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        public string? ConnectionId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual Usuario User { get; set; } = null!;
    }
}