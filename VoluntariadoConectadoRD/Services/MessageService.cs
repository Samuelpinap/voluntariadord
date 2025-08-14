using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Hubs;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Interfaces;
using System.Text.RegularExpressions;

namespace VoluntariadoConectadoRD.Services
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(int senderId, SendMessageDto messageDto);
        Task<ConversationMessagesDto> GetConversationMessagesAsync(int userId, string conversationId, int page = 1, int pageSize = 50);
        Task<ConversationListDto> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<MessageDto> EditMessageAsync(int messageId, int userId, EditMessageDto editDto);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
        Task<bool> MarkMessagesAsReadAsync(int userId, string conversationId);
        Task<ConversationDto> StartConversationAsync(int senderId, StartConversationDto startDto);
        Task<ConversationStatsDto> GetConversationStatsAsync(int userId);
        Task<bool> ArchiveConversationAsync(int userId, string conversationId);
        Task NotifyTyping(int senderId, int recipientId, string conversationId, bool isTyping);
        Task<string> GetOrCreateConversationId(int userId1, int userId2);
    }

    public class MessageService : IMessageService
    {
        private readonly DbContextApplication _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            DbContextApplication context,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService,
            IImageUploadService imageUploadService,
            ILogger<MessageService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public async Task<MessageDto> SendMessageAsync(int senderId, SendMessageDto messageDto)
        {
            try
            {
                // Validate recipient exists
                var recipient = await _context.Usuarios.FindAsync(messageDto.RecipientId);
                if (recipient == null)
                {
                    throw new ArgumentException("Recipient not found");
                }

                // Get or create conversation
                var conversationId = await GetOrCreateConversationId(senderId, messageDto.RecipientId);

                string? attachmentUrl = null;
                string? attachmentFileName = null;
                string? attachmentMimeType = null;
                long? attachmentSize = null;

                // Handle file attachment
                if (messageDto.Attachment != null)
                {
                    if (!MessageValidation.IsAllowedFileType(messageDto.Attachment.ContentType))
                    {
                        throw new ArgumentException("File type not allowed");
                    }

                    if (messageDto.Attachment.Length > MessageValidation.GetMaxFileSize())
                    {
                        throw new ArgumentException("File size exceeds maximum allowed");
                    }

                    // Upload the file
                    var fileName = await _imageUploadService.UploadFileAsync(messageDto.Attachment, "messages");
                    attachmentUrl = $"/uploads/messages/{fileName}";
                    attachmentFileName = messageDto.Attachment.FileName;
                    attachmentMimeType = messageDto.Attachment.ContentType;
                    attachmentSize = messageDto.Attachment.Length;

                    // Set message type based on file type
                    if (messageDto.Attachment.ContentType.StartsWith("image/"))
                    {
                        messageDto.Type = MessageType.Image;
                    }
                    else
                    {
                        messageDto.Type = MessageType.File;
                    }
                }

                var message = new Message
                {
                    SenderId = senderId,
                    RecipientId = messageDto.RecipientId,
                    Content = messageDto.Content,
                    Type = messageDto.Type,
                    ConversationId = conversationId,
                    ReplyToMessageId = messageDto.ReplyToMessageId,
                    AttachmentUrl = attachmentUrl,
                    AttachmentFileName = attachmentFileName,
                    AttachmentMimeType = attachmentMimeType,
                    AttachmentSize = attachmentSize,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);

                // Update conversation
                var conversation = await _context.Conversations.FindAsync(conversationId);
                if (conversation != null)
                {
                    conversation.LastMessageAt = DateTime.UtcNow;
                    conversation.LastMessageId = message.Id;

                    // Mark as unread for recipient
                    if (conversation.User1Id == messageDto.RecipientId)
                        conversation.User1HasUnread = true;
                    else
                        conversation.User2HasUnread = true;
                }

                await _context.SaveChangesAsync();

                // Load the complete message with sender info
                var completeMessage = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Recipient)
                    .Include(m => m.ReplyToMessage)
                        .ThenInclude(r => r.Sender)
                    .FirstAsync(m => m.Id == message.Id);

                var messageResult = MapToMessageDto(completeMessage, senderId);

                // Send real-time notification
                await _hubContext.Clients.Group($"User_{messageDto.RecipientId}")
                    .SendAsync("ReceiveMessage", messageResult);

                // Create notification for recipient
                var notification = new CreateNotificationDto
                {
                    RecipientId = messageDto.RecipientId,
                    SenderId = senderId,
                    Title = "Nuevo mensaje",
                    Message = $"Tienes un nuevo mensaje de {completeMessage.Sender.Nombre} {completeMessage.Sender.Apellido}",
                    Type = NotificationTypes.MESSAGE_RECEIVED,
                    ActionUrl = $"/Messages/{conversationId}",
                    Priority = NotificationPriority.Normal
                };

                await _notificationService.CreateNotificationAsync(notification);

                return messageResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", senderId, messageDto.RecipientId);
                throw;
            }
        }

        public async Task<ConversationMessagesDto> GetConversationMessagesAsync(int userId, string conversationId, int page = 1, int pageSize = 50)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .FirstOrDefaultAsync(c => c.Id == conversationId && (c.User1Id == userId || c.User2Id == userId));

                if (conversation == null)
                {
                    throw new ArgumentException("Conversation not found or access denied");
                }

                var otherUser = conversation.GetOtherUser(userId);
                var totalCount = await _context.Messages
                    .CountAsync(m => m.ConversationId == conversationId && !m.IsDeleted);

                var messages = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Recipient)
                    .Include(m => m.ReplyToMessage)
                        .ThenInclude(r => r.Sender)
                    .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Reverse to show oldest first
                messages.Reverse();

                var messageDtos = messages.Select(m => MapToMessageDto(m, userId)).ToList();

                // Check if other user is online
                var onlineStatus = await _notificationService.GetUserOnlineStatus(otherUser.Id);

                return new ConversationMessagesDto
                {
                    ConversationId = conversationId,
                    OtherUser = new UserBasicDto
                    {
                        Id = otherUser.Id,
                        Nombre = otherUser.Nombre,
                        Apellido = otherUser.Apellido,
                        ImagenUrl = otherUser.ImagenUrl
                    },
                    Messages = messageDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    IsOtherUserOnline = onlineStatus.IsOnline,
                    OtherUserLastSeen = onlineStatus.LastSeen
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation messages for user {UserId}, conversation {ConversationId}", userId, conversationId);
                throw;
            }
        }

        public async Task<ConversationListDto> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Conversations
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .Include(c => c.LastMessage)
                        .ThenInclude(m => m.Sender)
                    .Where(c => (c.User1Id == userId || c.User2Id == userId) && !c.IsArchived);

                var totalCount = await query.CountAsync();

                var conversations = await query
                    .OrderByDescending(c => c.LastMessageAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var conversationDtos = new List<ConversationDto>();
                var totalUnread = 0;

                foreach (var conv in conversations)
                {
                    var otherUser = conv.GetOtherUser(userId);
                    var hasUnread = conv.HasUnreadFor(userId);
                    
                    if (hasUnread)
                        totalUnread++;

                    var unreadCount = await _context.Messages
                        .CountAsync(m => m.ConversationId == conv.Id && 
                                   m.RecipientId == userId && 
                                   !m.IsRead && 
                                   !m.IsDeleted);

                    var onlineStatus = await _notificationService.GetUserOnlineStatus(otherUser.Id);

                    var conversationDto = new ConversationDto
                    {
                        Id = conv.Id,
                        OtherUser = new UserBasicDto
                        {
                            Id = otherUser.Id,
                            Nombre = otherUser.Nombre,
                            Apellido = otherUser.Apellido,
                            ImagenUrl = otherUser.ImagenUrl
                        },
                        LastMessage = conv.LastMessage != null ? MapToMessageDto(conv.LastMessage, userId) : null,
                        LastMessageAt = conv.LastMessageAt,
                        HasUnread = hasUnread,
                        UnreadCount = unreadCount,
                        IsOnline = onlineStatus.IsOnline,
                        CreatedAt = conv.CreatedAt
                    };

                    conversationDtos.Add(conversationDto);
                }

                return new ConversationListDto
                {
                    Conversations = conversationDtos,
                    TotalUnread = totalUnread,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                throw;
            }
        }

        public async Task<MessageDto> EditMessageAsync(int messageId, int userId, EditMessageDto editDto)
        {
            try
            {
                var message = await _context.Messages
                    .Include(m => m.Sender)
                    .Include(m => m.Recipient)
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId && !m.IsDeleted);

                if (message == null)
                {
                    throw new ArgumentException("Message not found or access denied");
                }

                // Only allow editing within 15 minutes
                if (DateTime.UtcNow - message.SentAt > TimeSpan.FromMinutes(15))
                {
                    throw new ArgumentException("Message can only be edited within 15 minutes");
                }

                message.Content = editDto.Content;
                message.EditedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = MapToMessageDto(message, userId);

                // Notify recipient of message edit
                await _hubContext.Clients.Group($"User_{message.RecipientId}")
                    .SendAsync("MessageEdited", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message {MessageId} by user {UserId}", messageId, userId);
                throw;
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId && !m.IsDeleted);

                if (message == null)
                {
                    return false;
                }

                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;
                message.Content = "[Mensaje eliminado]";

                await _context.SaveChangesAsync();

                // Notify recipient of message deletion
                await _hubContext.Clients.Group($"User_{message.RecipientId}")
                    .SendAsync("MessageDeleted", messageId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} by user {UserId}", messageId, userId);
                throw;
            }
        }

        public async Task<bool> MarkMessagesAsReadAsync(int userId, string conversationId)
        {
            try
            {
                var unreadMessages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId && 
                               m.RecipientId == userId && 
                               !m.IsRead && 
                               !m.IsDeleted)
                    .ToListAsync();

                var readTime = DateTime.UtcNow;
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = readTime;
                }

                // Update conversation unread status
                var conversation = await _context.Conversations.FindAsync(conversationId);
                if (conversation != null)
                {
                    if (conversation.User1Id == userId)
                        conversation.User1HasUnread = false;
                    else
                        conversation.User2HasUnread = false;
                }

                await _context.SaveChangesAsync();

                // Notify sender that messages were read
                foreach (var message in unreadMessages)
                {
                    await _hubContext.Clients.Group($"User_{message.SenderId}")
                        .SendAsync("MessageRead", new MessageReadStatusDto
                        {
                            MessageId = message.Id,
                            IsRead = true,
                            ReadAt = readTime
                        });
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for user {UserId}, conversation {ConversationId}", userId, conversationId);
                throw;
            }
        }

        public async Task<ConversationDto> StartConversationAsync(int senderId, StartConversationDto startDto)
        {
            try
            {
                var recipient = await _context.Usuarios.FindAsync(startDto.RecipientId);
                if (recipient == null)
                {
                    throw new ArgumentException("Recipient not found");
                }

                var conversationId = await GetOrCreateConversationId(senderId, startDto.RecipientId);

                // Send initial message
                var messageDto = new SendMessageDto
                {
                    RecipientId = startDto.RecipientId,
                    Content = startDto.InitialMessage
                };

                await SendMessageAsync(senderId, messageDto);

                var conversation = await _context.Conversations
                    .Include(c => c.User1)
                    .Include(c => c.User2)
                    .Include(c => c.LastMessage)
                    .FirstAsync(c => c.Id == conversationId);

                var otherUser = conversation.GetOtherUser(senderId);
                var onlineStatus = await _notificationService.GetUserOnlineStatus(otherUser.Id);

                return new ConversationDto
                {
                    Id = conversation.Id,
                    OtherUser = new UserBasicDto
                    {
                        Id = otherUser.Id,
                        Nombre = otherUser.Nombre,
                        Apellido = otherUser.Apellido,
                        ImagenUrl = otherUser.ImagenUrl
                    },
                    LastMessage = conversation.LastMessage != null ? MapToMessageDto(conversation.LastMessage, senderId) : null,
                    LastMessageAt = conversation.LastMessageAt,
                    HasUnread = false,
                    UnreadCount = 0,
                    IsOnline = onlineStatus.IsOnline,
                    CreatedAt = conversation.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting conversation from {SenderId} to {RecipientId}", senderId, startDto.RecipientId);
                throw;
            }
        }

        public async Task<ConversationStatsDto> GetConversationStatsAsync(int userId)
        {
            try
            {
                var totalConversations = await _context.Conversations
                    .CountAsync(c => (c.User1Id == userId || c.User2Id == userId) && !c.IsArchived);

                var unreadConversations = await _context.Conversations
                    .CountAsync(c => (c.User1Id == userId || c.User2Id == userId) && 
                               !c.IsArchived && 
                               ((c.User1Id == userId && c.User1HasUnread) || 
                                (c.User2Id == userId && c.User2HasUnread)));

                var totalUnreadMessages = await _context.Messages
                    .CountAsync(m => m.RecipientId == userId && !m.IsRead && !m.IsDeleted);

                var lastActivity = await _context.Messages
                    .Where(m => m.SenderId == userId || m.RecipientId == userId)
                    .MaxAsync(m => (DateTime?)m.SentAt);

                return new ConversationStatsDto
                {
                    TotalConversations = totalConversations,
                    UnreadConversations = unreadConversations,
                    TotalUnreadMessages = totalUnreadMessages,
                    LastActivity = lastActivity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation stats for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ArchiveConversationAsync(int userId, string conversationId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId && 
                                        (c.User1Id == userId || c.User2Id == userId));

                if (conversation == null)
                {
                    return false;
                }

                conversation.IsArchived = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving conversation {ConversationId} for user {UserId}", conversationId, userId);
                throw;
            }
        }

        public async Task NotifyTyping(int senderId, int recipientId, string conversationId, bool isTyping)
        {
            try
            {
                var sender = await _context.Usuarios.FindAsync(senderId);
                if (sender == null) return;

                var typingIndicator = new TypingIndicatorDto
                {
                    UserId = senderId,
                    UserName = $"{sender.Nombre} {sender.Apellido}",
                    ConversationId = conversationId,
                    IsTyping = isTyping
                };

                await _hubContext.Clients.Group($"User_{recipientId}")
                    .SendAsync("TypingIndicator", typingIndicator);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator from {SenderId} to {RecipientId}", senderId, recipientId);
            }
        }

        public async Task<string> GetOrCreateConversationId(int userId1, int userId2)
        {
            // Ensure consistent conversation ID format (lower ID first)
            var user1Id = Math.Min(userId1, userId2);
            var user2Id = Math.Max(userId1, userId2);
            var conversationId = $"{user1Id}_{user2Id}";

            var existingConversation = await _context.Conversations.FindAsync(conversationId);
            if (existingConversation == null)
            {
                var conversation = new Conversation
                {
                    Id = conversationId,
                    User1Id = user1Id,
                    User2Id = user2Id,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                };

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversationId;
        }

        private MessageDto MapToMessageDto(Message message, int currentUserId)
        {
            var dto = new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                RecipientId = message.RecipientId,
                Content = message.Content,
                Type = message.Type,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt,
                EditedAt = message.EditedAt,
                ReplyToMessageId = message.ReplyToMessageId,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentFileName = message.AttachmentFileName,
                AttachmentMimeType = message.AttachmentMimeType,
                AttachmentSize = message.AttachmentSize,
                ConversationId = message.ConversationId,
                Sender = new UserBasicDto
                {
                    Id = message.Sender.Id,
                    Nombre = message.Sender.Nombre,
                    Apellido = message.Sender.Apellido,
                    ImagenUrl = message.Sender.ImagenUrl
                },
                Recipient = new UserBasicDto
                {
                    Id = message.Recipient.Id,
                    Nombre = message.Recipient.Nombre,
                    Apellido = message.Recipient.Apellido,
                    ImagenUrl = message.Recipient.ImagenUrl
                },
                ReplyToMessage = message.ReplyToMessage != null ? new MessageDto
                {
                    Id = message.ReplyToMessage.Id,
                    Content = message.ReplyToMessage.Content,
                    Sender = new UserBasicDto
                    {
                        Id = message.ReplyToMessage.Sender.Id,
                        Nombre = message.ReplyToMessage.Sender.Nombre,
                        Apellido = message.ReplyToMessage.Sender.Apellido,
                        ImagenUrl = message.ReplyToMessage.Sender.ImagenUrl
                    }
                } : null,
                TimeAgo = GetTimeAgoText(message.SentAt),
                IsFromCurrentUser = message.SenderId == currentUserId,
                FormattedContent = FormatMessageContent(message.Content, message.Type)
            };

            return dto;
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
                return "ahora";
            }
        }

        private string FormatMessageContent(string content, MessageType type)
        {
            if (type == MessageType.System || type == MessageType.ApplicationUpdate)
            {
                return content;
            }

            // Basic URL link formatting
            var urlPattern = @"(https?://[^\s]+)";
            var formattedContent = Regex.Replace(content, urlPattern, "<a href=\"$1\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");

            // Basic line break formatting
            formattedContent = formattedContent.Replace("\n", "<br>");

            return formattedContent;
        }
    }
}