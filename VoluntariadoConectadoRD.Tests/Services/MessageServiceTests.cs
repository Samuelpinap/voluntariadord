using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Hubs;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Tests.Services
{
    public class MessageServiceTests : IDisposable
    {
        private readonly DbContextApplication _context;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IImageUploadService> _mockImageUploadService;
        private readonly Mock<ILogger<MessageService>> _mockLogger;
        private readonly MessageService _messageService;

        public MessageServiceTests()
        {
            var options = new DbContextOptionsBuilder<DbContextApplication>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DbContextApplication(options);
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockImageUploadService = new Mock<IImageUploadService>();
            _mockLogger = new Mock<ILogger<MessageService>>();

            _messageService = new MessageService(
                _context,
                _mockHubContext.Object,
                _mockNotificationService.Object,
                _mockImageUploadService.Object,
                _mockLogger.Object
            );
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task SendMessageAsync_ValidMessage_CreatesSuccessfully()
        {
            // Arrange
            var sender = new Usuario
            {
                Id = 1,
                Nombre = "Sender",
                Apellido = "User",
                Email = "sender@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var recipient = new Usuario
            {
                Id = 2,
                Nombre = "Recipient",
                Apellido = "User",
                Email = "recipient@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            await _context.Usuarios.AddRangeAsync(sender, recipient);
            await _context.SaveChangesAsync();

            var messageDto = new SendMessageDto
            {
                RecipientId = 2,
                Content = "Hello, this is a test message",
                Type = MessageType.Text
            };

            // Mock SignalR
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Mock NotificationService
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()))
                .ReturnsAsync(new NotificationDto());

            // Act
            var result = await _messageService.SendMessageAsync(1, messageDto);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(messageDto.Content);
            result.SenderId.Should().Be(1);
            result.RecipientId.Should().Be(2);
            result.Type.Should().Be(MessageType.Text);

            var savedMessage = await _context.Messages.FirstOrDefaultAsync();
            savedMessage.Should().NotBeNull();
            savedMessage!.Content.Should().Be(messageDto.Content);

            var conversation = await _context.Conversations.FirstOrDefaultAsync();
            conversation.Should().NotBeNull();
            conversation!.Id.Should().Be("1_2");
        }

        [Fact]
        public async Task GetOrCreateConversationId_NewConversation_CreatesConversation()
        {
            // Arrange
            var user1 = new Usuario
            {
                Id = 5,
                Nombre = "User1",
                Apellido = "Test",
                Email = "user1@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var user2 = new Usuario
            {
                Id = 3,
                Nombre = "User2",
                Apellido = "Test",
                Email = "user2@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            await _context.Usuarios.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.GetOrCreateConversationId(5, 3);

            // Assert
            result.Should().Be("3_5"); // Always lower ID first

            var conversation = await _context.Conversations.FindAsync("3_5");
            conversation.Should().NotBeNull();
            conversation!.User1Id.Should().Be(3);
            conversation.User2Id.Should().Be(5);
        }

        [Fact]
        public async Task GetOrCreateConversationId_ExistingConversation_ReturnsExistingId()
        {
            // Arrange
            var conversation = new Conversation
            {
                Id = "1_2",
                User1Id = 1,
                User2Id = 2,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _messageService.GetOrCreateConversationId(2, 1);

            // Assert
            result.Should().Be("1_2");

            var conversationCount = await _context.Conversations.CountAsync();
            conversationCount.Should().Be(1); // Should not create a new one
        }

        [Fact]
        public async Task GetUserConversationsAsync_ValidUser_ReturnsConversations()
        {
            // Arrange
            var user1 = new Usuario
            {
                Id = 1,
                Nombre = "User1",
                Apellido = "Test",
                Email = "user1@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var user2 = new Usuario
            {
                Id = 2,
                Nombre = "User2",
                Apellido = "Test",
                Email = "user2@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var user3 = new Usuario
            {
                Id = 3,
                Nombre = "User3",
                Apellido = "Test",
                Email = "user3@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var conversation1 = new Conversation
            {
                Id = "1_2",
                User1Id = 1,
                User2Id = 2,
                LastMessageAt = DateTime.UtcNow.AddHours(-1),
                User1HasUnread = false,
                User2HasUnread = true
            };

            var conversation2 = new Conversation
            {
                Id = "1_3",
                User1Id = 1,
                User2Id = 3,
                LastMessageAt = DateTime.UtcNow,
                User1HasUnread = true,
                User2HasUnread = false
            };

            var message1 = new Message
            {
                Id = 1,
                SenderId = 2,
                RecipientId = 1,
                Content = "Hello from user 2",
                ConversationId = "1_2",
                SentAt = DateTime.UtcNow.AddHours(-1)
            };

            var message2 = new Message
            {
                Id = 2,
                SenderId = 1,
                RecipientId = 3,
                Content = "Hello from user 1",
                ConversationId = "1_3",
                SentAt = DateTime.UtcNow
            };

            conversation1.LastMessageId = 1;
            conversation2.LastMessageId = 2;

            await _context.Usuarios.AddRangeAsync(user1, user2, user3);
            await _context.Conversations.AddRangeAsync(conversation1, conversation2);
            await _context.Messages.AddRangeAsync(message1, message2);
            await _context.SaveChangesAsync();

            // Mock NotificationService for online status
            _mockNotificationService.Setup(x => x.GetUserOnlineStatus(It.IsAny<int>()))
                .ReturnsAsync(new OnlineStatusDto { IsOnline = false });

            // Act
            var result = await _messageService.GetUserConversationsAsync(1, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().HaveCount(2);
            result.TotalUnread.Should().Be(1); // User 1 has unread in conversation with user 3
            result.Conversations.First().OtherUser.Nombre.Should().Be("User3"); // Most recent first
        }

        [Fact]
        public async Task MarkMessagesAsReadAsync_ValidConversation_MarksAsRead()
        {
            // Arrange
            var user1 = new Usuario
            {
                Id = 1,
                Nombre = "User1",
                Apellido = "Test",
                Email = "user1@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var user2 = new Usuario
            {
                Id = 2,
                Nombre = "User2",
                Apellido = "Test",
                Email = "user2@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var conversation = new Conversation
            {
                Id = "1_2",
                User1Id = 1,
                User2Id = 2,
                User1HasUnread = true,
                User2HasUnread = false
            };

            var message1 = new Message
            {
                SenderId = 2,
                RecipientId = 1,
                Content = "Unread message 1",
                ConversationId = "1_2",
                IsRead = false
            };

            var message2 = new Message
            {
                SenderId = 2,
                RecipientId = 1,
                Content = "Unread message 2",
                ConversationId = "1_2",
                IsRead = false
            };

            await _context.Usuarios.AddRangeAsync(user1, user2);
            await _context.Conversations.AddAsync(conversation);
            await _context.Messages.AddRangeAsync(message1, message2);
            await _context.SaveChangesAsync();

            // Mock SignalR
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Act
            var result = await _messageService.MarkMessagesAsReadAsync(1, "1_2");

            // Assert
            result.Should().BeTrue();

            var messages = await _context.Messages.Where(m => m.ConversationId == "1_2").ToListAsync();
            messages.All(m => m.IsRead).Should().BeTrue();
            messages.All(m => m.ReadAt != null).Should().BeTrue();

            var updatedConversation = await _context.Conversations.FindAsync("1_2");
            updatedConversation!.User1HasUnread.Should().BeFalse();
        }

        [Fact]
        public async Task EditMessageAsync_ValidMessage_EditsSuccessfully()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "User",
                Apellido = "Test",
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var message = new Message
            {
                Id = 1,
                SenderId = 1,
                RecipientId = 2,
                Content = "Original message",
                ConversationId = "1_2",
                SentAt = DateTime.UtcNow.AddMinutes(-5) // Within 15 minute edit window
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var editDto = new EditMessageDto
            {
                Content = "Edited message content"
            };

            // Mock SignalR
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Act
            var result = await _messageService.EditMessageAsync(1, 1, editDto);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(editDto.Content);
            result.EditedAt.Should().NotBeNull();

            var editedMessage = await _context.Messages.FindAsync(1);
            editedMessage!.Content.Should().Be(editDto.Content);
            editedMessage.EditedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task EditMessageAsync_MessageTooOld_ThrowsException()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "User",
                Apellido = "Test",
                Email = "user@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var message = new Message
            {
                Id = 1,
                SenderId = 1,
                RecipientId = 2,
                Content = "Original message",
                ConversationId = "1_2",
                SentAt = DateTime.UtcNow.AddHours(-1) // Outside 15 minute edit window
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var editDto = new EditMessageDto
            {
                Content = "Edited message content"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _messageService.EditMessageAsync(1, 1, editDto));

            exception.Message.Should().Be("Message can only be edited within 15 minutes");
        }

        [Fact]
        public async Task DeleteMessageAsync_ValidMessage_DeletesSuccessfully()
        {
            // Arrange
            var message = new Message
            {
                Id = 1,
                SenderId = 1,
                RecipientId = 2,
                Content = "Message to delete",
                ConversationId = "1_2",
                IsDeleted = false
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Mock SignalR
            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Act
            var result = await _messageService.DeleteMessageAsync(1, 1);

            // Assert
            result.Should().BeTrue();

            var deletedMessage = await _context.Messages.FindAsync(1);
            deletedMessage!.IsDeleted.Should().BeTrue();
            deletedMessage.DeletedAt.Should().NotBeNull();
            deletedMessage.Content.Should().Be("[Mensaje eliminado]");
        }
    }
}