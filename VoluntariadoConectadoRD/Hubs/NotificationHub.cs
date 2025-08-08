using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly INotificationService _notificationService;

        public NotificationHub(ILogger<NotificationHub> logger, INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                // Join user to their personal notification group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Mark user as online
                await _notificationService.SetUserOnlineStatus(userId, true);

                _logger.LogInformation("User {UserId} connected to SignalR hub with connection {ConnectionId}", 
                    userId, Context.ConnectionId);

                // Send unread notifications count
                var unreadCount = await _notificationService.GetUnreadNotificationsCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCount", unreadCount);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                // Remove user from their personal notification group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Mark user as offline (with a small delay to handle reconnections)
                await Task.Delay(TimeSpan.FromSeconds(5));
                await _notificationService.SetUserOnlineStatus(userId, false);

                _logger.LogInformation("User {UserId} disconnected from SignalR hub", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client can call this to join specific groups (e.g., organization groups)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} joined group {GroupName}", 
                Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Connection {ConnectionId} left group {GroupName}", 
                Context.ConnectionId, groupName);
        }

        // Mark notification as read
        public async Task MarkNotificationAsRead(int notificationId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
                
                // Send updated unread count
                var unreadCount = await _notificationService.GetUnreadNotificationsCountAsync(userId);
                await Clients.Caller.SendAsync("UnreadCount", unreadCount);
            }
        }

        // Mark all notifications as read
        public async Task MarkAllNotificationsAsRead()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                await Clients.Caller.SendAsync("UnreadCount", 0);
            }
        }

        // Get user's notifications
        public async Task GetNotifications(int page = 1, int pageSize = 20)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                await Clients.Caller.SendAsync("NotificationsList", notifications);
            }
        }

        // Send typing indicator for messaging
        public async Task StartTyping(int recipientId, string conversationId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                await Clients.Group($"User_{recipientId}").SendAsync("UserTyping", new
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    IsTyping = true
                });
            }
        }

        public async Task StopTyping(int recipientId, string conversationId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                await Clients.Group($"User_{recipientId}").SendAsync("UserTyping", new
                {
                    UserId = userId,
                    ConversationId = conversationId,
                    IsTyping = false
                });
            }
        }

        // Join conversation room for real-time messaging
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
            _logger.LogInformation("Connection {ConnectionId} joined conversation {ConversationId}", 
                Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
            _logger.LogInformation("Connection {ConnectionId} left conversation {ConversationId}", 
                Context.ConnectionId, conversationId);
        }
    }
}