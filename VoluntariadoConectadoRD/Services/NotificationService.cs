using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Hubs;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using System.Text.Json;

namespace VoluntariadoConectadoRD.Services
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto);
        Task<List<NotificationDto>> CreateBulkNotificationAsync(BulkNotificationDto notificationDto);
        Task<NotificationListDto> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllNotificationsAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        Task<bool> SetUserOnlineStatus(int userId, bool isOnline);
        Task<OnlineStatusDto> GetUserOnlineStatus(int userId);
        Task<List<OnlineStatusDto>> GetOnlineUsers();
        Task SendNotificationToUser(int userId, NotificationDto notification);
        Task SendNotificationToGroup(string groupName, NotificationDto notification);
        Task SendNotificationToAll(NotificationDto notification);
    }

    public class NotificationService : INotificationService
    {
        private readonly DbContextApplication _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            DbContextApplication context, 
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            try
            {
                var notification = new Notification
                {
                    RecipientId = notificationDto.RecipientId,
                    SenderId = notificationDto.SenderId,
                    Title = notificationDto.Title,
                    Message = notificationDto.Message,
                    Type = notificationDto.Type,
                    ActionUrl = notificationDto.ActionUrl,
                    Priority = notificationDto.Priority,
                    Data = notificationDto.Data,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Load the full notification with related data
                var createdNotification = await _context.Notifications
                    .Include(n => n.Sender)
                    .FirstOrDefaultAsync(n => n.Id == notification.Id);

                var notificationResult = MapToDto(createdNotification!);

                // Send real-time notification via SignalR
                await SendNotificationToUser(notificationDto.RecipientId, notificationResult);

                _logger.LogInformation("Notification created for user {UserId}: {Title}", 
                    notificationDto.RecipientId, notificationDto.Title);

                return notificationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", notificationDto.RecipientId);
                throw;
            }
        }

        public async Task<List<NotificationDto>> CreateBulkNotificationAsync(BulkNotificationDto notificationDto)
        {
            try
            {
                var notifications = new List<Notification>();
                var results = new List<NotificationDto>();

                foreach (var recipientId in notificationDto.RecipientIds)
                {
                    var notification = new Notification
                    {
                        RecipientId = recipientId,
                        SenderId = notificationDto.SenderId,
                        Title = notificationDto.Title,
                        Message = notificationDto.Message,
                        Type = notificationDto.Type,
                        ActionUrl = notificationDto.ActionUrl,
                        Priority = notificationDto.Priority,
                        Data = notificationDto.Data,
                        CreatedAt = DateTime.UtcNow
                    };

                    notifications.Add(notification);
                }

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                // Load with related data and send via SignalR
                foreach (var notification in notifications)
                {
                    var fullNotification = await _context.Notifications
                        .Include(n => n.Sender)
                        .FirstOrDefaultAsync(n => n.Id == notification.Id);

                    if (fullNotification != null)
                    {
                        var notificationDto_result = MapToDto(fullNotification);
                        results.Add(notificationDto_result);

                        // Send real-time notification
                        await SendNotificationToUser(fullNotification.RecipientId, notificationDto_result);
                    }
                }

                _logger.LogInformation("Bulk notifications created for {Count} users", notificationDto.RecipientIds.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                throw;
            }
        }

        public async Task<NotificationListDto> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Notifications
                    .Include(n => n.Sender)
                    .Where(n => n.RecipientId == userId)
                    .OrderByDescending(n => n.CreatedAt);

                var totalCount = await query.CountAsync();
                var unreadCount = await query.CountAsync(n => !n.IsRead);

                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new NotificationListDto
                {
                    Notifications = notificationDtos,
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            try
            {
                return await _context.Notifications
                    .CountAsync(n => n.RecipientId == userId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

                if (notification != null && !notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", 
                    notificationId, userId);
                throw;
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.RecipientId == userId && !n.IsRead)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

                if (notification != null)
                {
                    _context.Notifications.Remove(notification);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", 
                    notificationId, userId);
                throw;
            }
        }

        public async Task<bool> SetUserOnlineStatus(int userId, bool isOnline)
        {
            try
            {
                var userStatus = await _context.UserOnlineStatuses
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (userStatus == null)
                {
                    userStatus = new UserOnlineStatus { UserId = userId };
                    _context.UserOnlineStatuses.Add(userStatus);
                }

                userStatus.IsOnline = isOnline;
                userStatus.LastSeen = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting online status for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OnlineStatusDto> GetUserOnlineStatus(int userId)
        {
            try
            {
                var status = await _context.UserOnlineStatuses
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (status == null)
                {
                    return new OnlineStatusDto
                    {
                        UserId = userId,
                        IsOnline = false,
                        LastSeen = DateTime.UtcNow,
                        LastSeenText = "Nunca conectado"
                    };
                }

                return new OnlineStatusDto
                {
                    UserId = userId,
                    IsOnline = status.IsOnline,
                    LastSeen = status.LastSeen,
                    LastSeenText = GetTimeAgoText(status.LastSeen)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online status for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<OnlineStatusDto>> GetOnlineUsers()
        {
            try
            {
                var onlineUsers = await _context.UserOnlineStatuses
                    .Where(u => u.IsOnline)
                    .Include(u => u.User)
                    .Select(u => new OnlineStatusDto
                    {
                        UserId = u.UserId,
                        IsOnline = u.IsOnline,
                        LastSeen = u.LastSeen,
                        LastSeenText = "En línea"
                    })
                    .ToListAsync();

                return onlineUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online users");
                throw;
            }
        }

        public async Task SendNotificationToUser(int userId, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", notification);

                // Also update the unread count
                var unreadCount = await GetUnreadNotificationsCountAsync(userId);
                await _hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("UnreadCount", unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToGroup(string groupName, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to group {GroupName}", groupName);
            }
        }

        public async Task SendNotificationToAll(NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                ActionUrl = notification.ActionUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                Priority = notification.Priority,
                Data = notification.Data,
                TimeAgo = GetTimeAgoText(notification.CreatedAt),
                Icon = GetNotificationIcon(notification.Type),
                Color = GetNotificationColor(notification.Type, notification.Priority),
                Sender = notification.Sender != null ? new UserBasicDto
                {
                    Id = notification.Sender.Id,
                    Nombre = notification.Sender.Nombre,
                    Apellido = notification.Sender.Apellido,
                    ImagenUrl = notification.Sender.ProfileImageUrl
                } : null
            };
        }

        private string GetTimeAgoText(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.Days > 0)
            {
                return $"hace {timeSpan.Days} día{(timeSpan.Days > 1 ? "s" : "")}";
            }
            else if (timeSpan.Hours > 0)
            {
                return $"hace {timeSpan.Hours} hora{(timeSpan.Hours > 1 ? "s" : "")}";
            }
            else if (timeSpan.Minutes > 0)
            {
                return $"hace {timeSpan.Minutes} minuto{(timeSpan.Minutes > 1 ? "s" : "")}";
            }
            else
            {
                return "hace un momento";
            }
        }

        private string GetNotificationIcon(string type)
        {
            return type switch
            {
                NotificationTypes.APPLICATION_SUBMITTED => "bi-person-plus",
                NotificationTypes.APPLICATION_APPROVED => "bi-check-circle",
                NotificationTypes.APPLICATION_REJECTED => "bi-x-circle",
                NotificationTypes.APPLICATION_CANCELLED => "bi-dash-circle",
                NotificationTypes.OPPORTUNITY_CREATED => "bi-calendar-plus",
                NotificationTypes.OPPORTUNITY_UPDATED => "bi-calendar-check",
                NotificationTypes.OPPORTUNITY_CANCELLED => "bi-calendar-x",
                NotificationTypes.MESSAGE_RECEIVED => "bi-chat-dots",
                NotificationTypes.PROFILE_VERIFIED => "bi-patch-check",
                NotificationTypes.REMINDER_UPCOMING => "bi-bell",
                NotificationTypes.SYSTEM_ANNOUNCEMENT => "bi-megaphone",
                NotificationTypes.BADGE_EARNED => "bi-award",
                NotificationTypes.WELCOME => "bi-heart",
                _ => "bi-bell"
            };
        }

        private string GetNotificationColor(string type, NotificationPriority priority)
        {
            if (priority == NotificationPriority.Urgent)
                return "danger";

            return type switch
            {
                NotificationTypes.APPLICATION_APPROVED => "success",
                NotificationTypes.APPLICATION_REJECTED => "warning",
                NotificationTypes.APPLICATION_CANCELLED => "secondary",
                NotificationTypes.OPPORTUNITY_CREATED => "primary",
                NotificationTypes.MESSAGE_RECEIVED => "info",
                NotificationTypes.PROFILE_VERIFIED => "success",
                NotificationTypes.BADGE_EARNED => "warning",
                NotificationTypes.WELCOME => "primary",
                _ => priority == NotificationPriority.High ? "warning" : "primary"
            };
        }
    }
}