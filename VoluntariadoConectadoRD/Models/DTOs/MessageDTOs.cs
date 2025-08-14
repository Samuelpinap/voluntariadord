using System.ComponentModel.DataAnnotations;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public int? ReplyToMessageId { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public string? AttachmentMimeType { get; set; }
        public long? AttachmentSize { get; set; }
        public string ConversationId { get; set; } = string.Empty;
        public UserBasicDto Sender { get; set; } = new UserBasicDto();
        public UserBasicDto Recipient { get; set; } = new UserBasicDto();
        public MessageDto? ReplyToMessage { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsFromCurrentUser { get; set; }
        public string FormattedContent { get; set; } = string.Empty;
    }

    public class SendMessageDto
    {
        [Required]
        public int RecipientId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; } = MessageType.Text;

        public int? ReplyToMessageId { get; set; }

        // For file attachments
        public IFormFile? Attachment { get; set; }
    }

    public class ConversationDto
    {
        public string Id { get; set; } = string.Empty;
        public UserBasicDto OtherUser { get; set; } = new UserBasicDto();
        public MessageDto? LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public bool HasUnread { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsOnline { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationListDto
    {
        public List<ConversationDto> Conversations { get; set; } = new List<ConversationDto>();
        public int TotalUnread { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public class ConversationMessagesDto
    {
        public string ConversationId { get; set; } = string.Empty;
        public UserBasicDto OtherUser { get; set; } = new UserBasicDto();
        public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
        public bool IsOtherUserOnline { get; set; }
        public DateTime? OtherUserLastSeen { get; set; }
        public bool IsTyping { get; set; }
    }

    public class MessageReadStatusDto
    {
        public int MessageId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class ConversationStatsDto
    {
        public int TotalConversations { get; set; }
        public int UnreadConversations { get; set; }
        public int TotalUnreadMessages { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class StartConversationDto
    {
        [Required]
        public int RecipientId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string InitialMessage { get; set; } = string.Empty;

        public string? Subject { get; set; }
    }

    public class EditMessageDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public class TypingIndicatorDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ConversationId { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }

    // Helper class for message templates
    public static class MessageTemplates
    {
        public static SendMessageDto ApplicationApproved(string volunteerName, string opportunityTitle)
        {
            return new SendMessageDto
            {
                Content = $"¡Felicidades! Tu postulación para '{opportunityTitle}' ha sido aprobada. Te contactaremos pronto con más detalles.",
                Type = MessageType.System
            };
        }

        public static SendMessageDto ApplicationRejected(string volunteerName, string opportunityTitle, string? reason = null)
        {
            var content = $"Gracias por tu interés en '{opportunityTitle}'. Desafortunadamente, no has sido seleccionado para esta oportunidad.";
            if (!string.IsNullOrEmpty(reason))
            {
                content += $" Motivo: {reason}";
            }
            content += " ¡No te desanimes! Hay muchas otras oportunidades disponibles.";

            return new SendMessageDto
            {
                Content = content,
                Type = MessageType.ApplicationUpdate
            };
        }

        public static SendMessageDto NewOpportunityAnnouncement(string organizationName, string opportunityTitle)
        {
            return new SendMessageDto
            {
                Content = $"¡Nueva oportunidad disponible! {organizationName} ha publicado '{opportunityTitle}'. ¡Postúlate ahora!",
                Type = MessageType.System
            };
        }

        public static SendMessageDto EventReminder(string opportunityTitle, DateTime eventDate)
        {
            return new SendMessageDto
            {
                Content = $"Recordatorio: Tu evento '{opportunityTitle}' será mañana ({eventDate:dd/MM/yyyy HH:mm}). ¡No olvides prepararte!",
                Type = MessageType.System
            };
        }
    }

    // Message validation helpers
    public static class MessageValidation
    {
        public static bool IsValidMessageType(MessageType type)
        {
            return Enum.IsDefined(typeof(MessageType), type);
        }

        public static bool IsAllowedFileType(string mimeType)
        {
            var allowedTypes = new[]
            {
                "image/jpeg", "image/png", "image/gif", "image/webp",
                "application/pdf", "text/plain",
                "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return allowedTypes.Contains(mimeType?.ToLowerInvariant());
        }

        public static long GetMaxFileSize()
        {
            return 5 * 1024 * 1024; // 5MB
        }
    }
}