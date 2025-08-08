using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public NotificationPriority Priority { get; set; }
        public UserBasicDto? Sender { get; set; }
        public string? Data { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class CreateNotificationDto
    {
        public int RecipientId { get; set; }
        public int? SenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public string? Data { get; set; }
    }

    public class NotificationListDto
    {
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public class UserBasicDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string FullName => $"{Nombre} {Apellido}".Trim();
    }

    public class OnlineStatusDto
    {
        public int UserId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
        public string LastSeenText { get; set; } = string.Empty;
    }

    public class BulkNotificationDto
    {
        public List<int> RecipientIds { get; set; } = new List<int>();
        public int? SenderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public string? Data { get; set; }
    }

    // Predefined notification types for consistency
    public static class NotificationTypes
    {
        public const string APPLICATION_SUBMITTED = "application_submitted";
        public const string APPLICATION_APPROVED = "application_approved";
        public const string APPLICATION_REJECTED = "application_rejected";
        public const string APPLICATION_CANCELLED = "application_cancelled";
        public const string OPPORTUNITY_CREATED = "opportunity_created";
        public const string OPPORTUNITY_UPDATED = "opportunity_updated";
        public const string OPPORTUNITY_CANCELLED = "opportunity_cancelled";
        public const string MESSAGE_RECEIVED = "message_received";
        public const string PROFILE_VERIFIED = "profile_verified";
        public const string REMINDER_UPCOMING = "reminder_upcoming";
        public const string SYSTEM_ANNOUNCEMENT = "system_announcement";
        public const string BADGE_EARNED = "badge_earned";
        public const string WELCOME = "welcome";
    }

    // Helper class for notification templates
    public static class NotificationTemplates
    {
        public static CreateNotificationDto ApplicationSubmitted(int organizationUserId, int volunteerId, string volunteerName, string opportunityTitle, int opportunityId)
        {
            return new CreateNotificationDto
            {
                RecipientId = organizationUserId,
                SenderId = volunteerId,
                Title = "Nueva postulación recibida",
                Message = $"{volunteerName} se ha postulado a {opportunityTitle}",
                Type = NotificationTypes.APPLICATION_SUBMITTED,
                ActionUrl = $"/Events/Applicants/{opportunityId}",
                Priority = NotificationPriority.Normal
            };
        }

        public static CreateNotificationDto ApplicationApproved(int volunteerId, int organizationUserId, string opportunityTitle, int opportunityId)
        {
            return new CreateNotificationDto
            {
                RecipientId = volunteerId,
                SenderId = organizationUserId,
                Title = "¡Postulación aprobada!",
                Message = $"Tu postulación a {opportunityTitle} ha sido aprobada",
                Type = NotificationTypes.APPLICATION_APPROVED,
                ActionUrl = $"/Events/Details/{opportunityId}",
                Priority = NotificationPriority.High
            };
        }

        public static CreateNotificationDto ApplicationRejected(int volunteerId, int organizationUserId, string opportunityTitle, int opportunityId, string? reason = null)
        {
            var message = $"Tu postulación a {opportunityTitle} no ha sido seleccionada";
            if (!string.IsNullOrEmpty(reason))
            {
                message += $". Motivo: {reason}";
            }

            return new CreateNotificationDto
            {
                RecipientId = volunteerId,
                SenderId = organizationUserId,
                Title = "Postulación no seleccionada",
                Message = message,
                Type = NotificationTypes.APPLICATION_REJECTED,
                ActionUrl = $"/Events/Details/{opportunityId}",
                Priority = NotificationPriority.Normal
            };
        }

        public static CreateNotificationDto OpportunityCreated(int volunteerId, string organizationName, string opportunityTitle, int opportunityId)
        {
            return new CreateNotificationDto
            {
                RecipientId = volunteerId,
                Title = "Nueva oportunidad disponible",
                Message = $"{organizationName} ha publicado una nueva oportunidad: {opportunityTitle}",
                Type = NotificationTypes.OPPORTUNITY_CREATED,
                ActionUrl = $"/Events/Details/{opportunityId}",
                Priority = NotificationPriority.Normal
            };
        }

        public static CreateNotificationDto Welcome(int userId, string userName)
        {
            return new CreateNotificationDto
            {
                RecipientId = userId,
                Title = "¡Bienvenido a Voluntariado Conectado RD!",
                Message = $"Hola {userName}, gracias por unirte a nuestra plataforma. ¡Comienza a explorar oportunidades de voluntariado!",
                Type = NotificationTypes.WELCOME,
                ActionUrl = "/Dashboard/Index",
                Priority = NotificationPriority.Normal
            };
        }

        public static CreateNotificationDto ReminderUpcoming(int userId, string opportunityTitle, DateTime opportunityDate, int opportunityId)
        {
            return new CreateNotificationDto
            {
                RecipientId = userId,
                Title = "Recordatorio de evento",
                Message = $"Tu evento '{opportunityTitle}' será mañana ({opportunityDate:dd/MM/yyyy HH:mm})",
                Type = NotificationTypes.REMINDER_UPCOMING,
                ActionUrl = $"/Events/Details/{opportunityId}",
                Priority = NotificationPriority.High
            };
        }
    }
}