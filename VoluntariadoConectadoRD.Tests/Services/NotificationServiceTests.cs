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
    public class NotificationServiceTests : IDisposable
    {
        private readonly DbContextApplication _context;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<DbContextApplication>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DbContextApplication(options);
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockLogger = new Mock<ILogger<NotificationService>>();

            _notificationService = new NotificationService(_context, _mockHubContext.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateNotificationAsync_ValidNotification_CreatesSuccessfully()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();

            var createDto = new CreateNotificationDto
            {
                RecipientId = 1,
                Title = "Test Notification",
                Message = "This is a test notification",
                Type = NotificationTypes.WELCOME,
                Priority = NotificationPriority.Normal
            };

            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Act
            var result = await _notificationService.CreateNotificationAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(createDto.Title);
            result.Message.Should().Be(createDto.Message);
            result.Type.Should().Be(createDto.Type);
            result.Priority.Should().Be(createDto.Priority);

            var savedNotification = await _context.Notifications.FirstOrDefaultAsync();
            savedNotification.Should().NotBeNull();
            savedNotification!.RecipientId.Should().Be(1);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ValidUser_ReturnsNotifications()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var notification1 = new Notification
            {
                RecipientId = 1,
                Title = "Notification 1",
                Message = "Message 1",
                Type = NotificationTypes.WELCOME,
                Priority = NotificationPriority.Normal,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };

            var notification2 = new Notification
            {
                RecipientId = 1,
                Title = "Notification 2",
                Message = "Message 2",
                Type = NotificationTypes.BADGE_EARNED,
                Priority = NotificationPriority.High,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                IsRead = true
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Notifications.AddRangeAsync(notification1, notification2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(1, 1, 10);

            // Assert
            result.Should().NotBeNull();
            result.Notifications.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.UnreadCount.Should().Be(1);
            result.Notifications.First().Title.Should().Be("Notification 2"); // Most recent first
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ValidNotification_MarksAsRead()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var notification = new Notification
            {
                Id = 1,
                RecipientId = 1,
                Title = "Test Notification",
                Message = "Test Message",
                Type = NotificationTypes.WELCOME,
                Priority = NotificationPriority.Normal,
                IsRead = false
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationService.MarkNotificationAsReadAsync(1, 1);

            // Assert
            result.Should().BeTrue();

            var updatedNotification = await _context.Notifications.FindAsync(1);
            updatedNotification!.IsRead.Should().BeTrue();
            updatedNotification.ReadAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUnreadNotificationsCountAsync_ValidUser_ReturnsCorrectCount()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var notification1 = new Notification
            {
                RecipientId = 1,
                Title = "Unread 1",
                Message = "Message 1",
                Type = NotificationTypes.WELCOME,
                IsRead = false
            };

            var notification2 = new Notification
            {
                RecipientId = 1,
                Title = "Read 1",
                Message = "Message 2",
                Type = NotificationTypes.WELCOME,
                IsRead = true
            };

            var notification3 = new Notification
            {
                RecipientId = 1,
                Title = "Unread 2",
                Message = "Message 3",
                Type = NotificationTypes.WELCOME,
                IsRead = false
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Notifications.AddRangeAsync(notification1, notification2, notification3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationService.GetUnreadNotificationsCountAsync(1);

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task SetUserOnlineStatus_NewUser_CreatesStatus()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationService.SetUserOnlineStatus(1, true);

            // Assert
            result.Should().BeTrue();

            var status = await _context.UserOnlineStatuses.FirstOrDefaultAsync(u => u.UserId == 1);
            status.Should().NotBeNull();
            status!.IsOnline.Should().BeTrue();
        }

        [Fact]
        public async Task CreateBulkNotificationAsync_ValidData_CreatesMultipleNotifications()
        {
            // Arrange
            var user1 = new Usuario
            {
                Id = 1,
                Nombre = "Test1",
                Apellido = "User1",
                Email = "test1@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var user2 = new Usuario
            {
                Id = 2,
                Nombre = "Test2",
                Apellido = "User2",
                Email = "test2@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            await _context.Usuarios.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            var bulkDto = new BulkNotificationDto
            {
                RecipientIds = new List<int> { 1, 2 },
                Title = "Bulk Notification",
                Message = "This is a bulk notification",
                Type = NotificationTypes.SYSTEM_ANNOUNCEMENT,
                Priority = NotificationPriority.High
            };

            var mockClients = new Mock<IHubCallerClients>();
            var mockGroup = new Mock<IClientProxy>();
            
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);

            // Act
            var result = await _notificationService.CreateBulkNotificationAsync(bulkDto);

            // Assert
            result.Should().HaveCount(2);
            result.All(n => n.Title == bulkDto.Title).Should().BeTrue();
            result.All(n => n.Message == bulkDto.Message).Should().BeTrue();

            var notifications = await _context.Notifications.ToListAsync();
            notifications.Should().HaveCount(2);
        }

        [Fact]
        public async Task MarkAllNotificationsAsReadAsync_ValidUser_MarksAllAsRead()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var notifications = new List<Notification>
            {
                new() { RecipientId = 1, Title = "N1", Message = "M1", Type = NotificationTypes.WELCOME, IsRead = false },
                new() { RecipientId = 1, Title = "N2", Message = "M2", Type = NotificationTypes.WELCOME, IsRead = false },
                new() { RecipientId = 1, Title = "N3", Message = "M3", Type = NotificationTypes.WELCOME, IsRead = true }
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationService.MarkAllNotificationsAsReadAsync(1);

            // Assert
            result.Should().BeTrue();

            var allNotifications = await _context.Notifications.Where(n => n.RecipientId == 1).ToListAsync();
            allNotifications.All(n => n.IsRead).Should().BeTrue();
            allNotifications.Where(n => n.ReadAt != null).Should().HaveCount(3);
        }
    }
}