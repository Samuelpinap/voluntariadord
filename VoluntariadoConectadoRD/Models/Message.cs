using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoluntariadoConectadoRD.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int RecipientId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public DateTime? EditedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // For replies/threads
        public int? ReplyToMessageId { get; set; }

        // For file attachments
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentMimeType { get; set; }
        public long? AttachmentSize { get; set; }

        // Conversation grouping
        public string ConversationId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("SenderId")]
        public virtual Usuario Sender { get; set; } = null!;

        [ForeignKey("RecipientId")]
        public virtual Usuario Recipient { get; set; } = null!;

        [ForeignKey("ReplyToMessageId")]
        public virtual Message? ReplyToMessage { get; set; }

        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    }

    public enum MessageType
    {
        Text = 1,
        Image = 2,
        File = 3,
        System = 4,
        ApplicationUpdate = 5
    }

    public class Conversation
    {
        [Key]
        public string Id { get; set; } = string.Empty; // Format: "user1_user2" (always lower ID first)

        [Required]
        public int User1Id { get; set; }

        [Required]
        public int User2Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public int? LastMessageId { get; set; }

        public bool User1HasUnread { get; set; } = false;
        public bool User2HasUnread { get; set; } = false;

        public DateTime? User1LastSeen { get; set; }
        public DateTime? User2LastSeen { get; set; }

        public bool IsArchived { get; set; } = false;

        // Navigation properties
        [ForeignKey("User1Id")]
        public virtual Usuario User1 { get; set; } = null!;

        [ForeignKey("User2Id")]
        public virtual Usuario User2 { get; set; } = null!;

        [ForeignKey("LastMessageId")]
        public virtual Message? LastMessage { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        // Helper methods
        public bool HasUnreadFor(int userId)
        {
            return userId == User1Id ? User1HasUnread : User2HasUnread;
        }

        public int GetOtherUserId(int currentUserId)
        {
            return currentUserId == User1Id ? User2Id : User1Id;
        }

        public Usuario GetOtherUser(int currentUserId)
        {
            return currentUserId == User1Id ? User2 : User1;
        }
    }
}