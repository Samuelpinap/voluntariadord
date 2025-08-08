using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Tests.Integration
{
    public class NotificationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly DbContextApplication _context;
        private readonly JwtService _jwtService;

        public NotificationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the database with in-memory database for testing
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DbContextApplication>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<DbContextApplication>(options =>
                        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
                });
            });

            _client = _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<DbContextApplication>();
            _jwtService = _scope.ServiceProvider.GetRequiredService<JwtService>();
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _client?.Dispose();
        }

        private async Task<string> GetJwtTokenAsync(int userId, UserRole role)
        {
            var user = new Usuario
            {
                Id = userId,
                Nombre = "Test",
                Apellido = "User",
                Email = $"test{userId}@example.com",
                PasswordHash = "hashedpassword",
                Rol = role,
                Estatus = UserStatus.Activo
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return _jwtService.GenerateToken(user);
        }

        private void SetAuthorizationHeader(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task CompleteNotificationWorkflow_CreatesAndManagesNotifications()
        {
            // Arrange
            var token = await GetJwtTokenAsync(1, UserRole.Voluntario);
            SetAuthorizationHeader(token);

            // Act 1: Create a notification (this would normally happen automatically)
            var createNotificationDto = new CreateNotificationDto
            {
                RecipientId = 1,
                Title = "Welcome to the platform!",
                Message = "Thank you for joining our volunteer community.",
                Type = NotificationTypes.WELCOME,
                Priority = NotificationPriority.Normal
            };

            var createContent = new StringContent(
                JsonSerializer.Serialize(createNotificationDto),
                Encoding.UTF8,
                "application/json");

            // This endpoint would normally be internal, but testing the flow
            var notification = new Notification
            {
                RecipientId = 1,
                Title = createNotificationDto.Title,
                Message = createNotificationDto.Message,
                Type = createNotificationDto.Type,
                Priority = createNotificationDto.Priority,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act 2: Get user notifications
            var getResponse = await _client.GetAsync("/api/notification");
            getResponse.EnsureSuccessStatusCode();

            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getResult = JsonSerializer.Deserialize<ApiResponseDto<NotificationListDto>>(getContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Should have the notification
            getResult.Should().NotBeNull();
            getResult!.Success.Should().BeTrue();
            getResult.Data.Should().NotBeNull();
            getResult.Data!.Notifications.Should().HaveCount(1);
            getResult.Data.UnreadCount.Should().Be(1);
            
            var notificationDto = getResult.Data.Notifications.First();
            notificationDto.Title.Should().Be("Welcome to the platform!");
            notificationDto.IsRead.Should().BeFalse();

            // Act 3: Mark notification as read
            var markReadResponse = await _client.PutAsync($"/api/notification/{notification.Id}/read", null);
            markReadResponse.EnsureSuccessStatusCode();

            // Act 4: Get notifications again to verify it's marked as read
            var getReadResponse = await _client.GetAsync("/api/notification");
            getReadResponse.EnsureSuccessStatusCode();

            var getReadContent = await getReadResponse.Content.ReadAsStringAsync();
            var getReadResult = JsonSerializer.Deserialize<ApiResponseDto<NotificationListDto>>(getReadContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Notification should be marked as read
            getReadResult.Should().NotBeNull();
            getReadResult!.Data.Should().NotBeNull();
            getReadResult.Data!.UnreadCount.Should().Be(0);
            getReadResult.Data.Notifications.First().IsRead.Should().BeTrue();

            // Act 5: Get unread count
            var unreadResponse = await _client.GetAsync("/api/notification/unread-count");
            unreadResponse.EnsureSuccessStatusCode();

            var unreadContent = await unreadResponse.Content.ReadAsStringAsync();
            var unreadResult = JsonSerializer.Deserialize<ApiResponseDto<int>>(unreadContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Unread count should be 0
            unreadResult.Should().NotBeNull();
            unreadResult!.Success.Should().BeTrue();
            unreadResult.Data.Should().Be(0);
        }

        [Fact]
        public async Task CompleteMessagingWorkflow_SendsAndReceivesMessages()
        {
            // Arrange
            var senderToken = await GetJwtTokenAsync(1, UserRole.Voluntario);
            var recipientToken = await GetJwtTokenAsync(2, UserRole.Organizacion);

            // Act 1: Send a message
            SetAuthorizationHeader(senderToken);
            
            var sendMessageDto = new SendMessageDto
            {
                RecipientId = 2,
                Content = "Hello, I'm interested in volunteering with your organization!",
                Type = MessageType.Text
            };

            var sendContent = new StringContent(
                JsonSerializer.Serialize(sendMessageDto),
                Encoding.UTF8,
                "application/json");

            var sendResponse = await _client.PostAsync("/api/message/send", sendContent);
            sendResponse.EnsureSuccessStatusCode();

            var sendResult = await sendResponse.Content.ReadAsStringAsync();
            var messageResult = JsonSerializer.Deserialize<ApiResponseDto<MessageDto>>(sendResult,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Message should be sent
            messageResult.Should().NotBeNull();
            messageResult!.Success.Should().BeTrue();
            messageResult.Data.Should().NotBeNull();
            messageResult.Data!.Content.Should().Be("Hello, I'm interested in volunteering with your organization!");
            messageResult.Data.SenderId.Should().Be(1);
            messageResult.Data.RecipientId.Should().Be(2);

            // Act 2: Get conversations for sender
            var senderConversationsResponse = await _client.GetAsync("/api/message/conversations");
            senderConversationsResponse.EnsureSuccessStatusCode();

            var senderConversationsContent = await senderConversationsResponse.Content.ReadAsStringAsync();
            var senderConversationsResult = JsonSerializer.Deserialize<ApiResponseDto<ConversationListDto>>(senderConversationsContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Sender should have one conversation
            senderConversationsResult.Should().NotBeNull();
            senderConversationsResult!.Success.Should().BeTrue();
            senderConversationsResult.Data.Should().NotBeNull();
            senderConversationsResult.Data!.Conversations.Should().HaveCount(1);
            
            var conversation = senderConversationsResult.Data.Conversations.First();
            conversation.OtherUser.Should().NotBeNull();
            conversation.LastMessage.Should().NotBeNull();
            conversation.LastMessage!.Content.Should().Be("Hello, I'm interested in volunteering with your organization!");

            // Act 3: Switch to recipient and get conversations
            SetAuthorizationHeader(recipientToken);
            
            var recipientConversationsResponse = await _client.GetAsync("/api/message/conversations");
            recipientConversationsResponse.EnsureSuccessStatusCode();

            var recipientConversationsContent = await recipientConversationsResponse.Content.ReadAsStringAsync();
            var recipientConversationsResult = JsonSerializer.Deserialize<ApiResponseDto<ConversationListDto>>(recipientConversationsContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Recipient should also have the conversation
            recipientConversationsResult.Should().NotBeNull();
            recipientConversationsResult!.Success.Should().BeTrue();
            recipientConversationsResult.Data!.Conversations.Should().HaveCount(1);
            recipientConversationsResult.Data.TotalUnread.Should().Be(1); // Should have unread message

            // Act 4: Get messages in conversation
            var conversationId = conversation.Id;
            var messagesResponse = await _client.GetAsync($"/api/message/conversation/{conversationId}");
            messagesResponse.EnsureSuccessStatusCode();

            var messagesContent = await messagesResponse.Content.ReadAsStringAsync();
            var messagesResult = JsonSerializer.Deserialize<ApiResponseDto<ConversationMessagesDto>>(messagesContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Should have one message
            messagesResult.Should().NotBeNull();
            messagesResult!.Success.Should().BeTrue();
            messagesResult.Data.Should().NotBeNull();
            messagesResult.Data!.Messages.Should().HaveCount(1);
            messagesResult.Data.Messages.First().Content.Should().Be("Hello, I'm interested in volunteering with your organization!");

            // Act 5: Mark messages as read
            var markReadResponse = await _client.PutAsync($"/api/message/conversation/{conversationId}/read", null);
            markReadResponse.EnsureSuccessStatusCode();

            // Act 6: Send a reply
            var replyDto = new SendMessageDto
            {
                RecipientId = 1,
                Content = "Thank you for your interest! We'd love to have you volunteer with us.",
                Type = MessageType.Text
            };

            var replyContent = new StringContent(
                JsonSerializer.Serialize(replyDto),
                Encoding.UTF8,
                "application/json");

            var replyResponse = await _client.PostAsync("/api/message/send", replyContent);
            replyResponse.EnsureSuccessStatusCode();

            // Act 7: Get updated conversation messages
            var updatedMessagesResponse = await _client.GetAsync($"/api/message/conversation/{conversationId}");
            updatedMessagesResponse.EnsureSuccessStatusCode();

            var updatedMessagesContent = await updatedMessagesResponse.Content.ReadAsStringAsync();
            var updatedMessagesResult = JsonSerializer.Deserialize<ApiResponseDto<ConversationMessagesDto>>(updatedMessagesContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Should have two messages
            updatedMessagesResult.Should().NotBeNull();
            updatedMessagesResult!.Success.Should().BeTrue();
            updatedMessagesResult.Data!.Messages.Should().HaveCount(2);
            updatedMessagesResult.Data.Messages.Should().Contain(m => m.Content == "Thank you for your interest! We'd love to have you volunteer with us.");
        }

        [Fact]
        public async Task BadgeAwardingWorkflow_AwardsBadgeOnActivityCompletion()
        {
            // Arrange
            var adminToken = await GetJwtTokenAsync(1, UserRole.Administrador);
            var volunteerToken = await GetJwtTokenAsync(2, UserRole.Voluntario);

            // Create organization and opportunity
            var organization = new Organizacion
            {
                Id = 1,
                Nombre = "Test Organization",
                Descripcion = "Test organization for integration tests",
                Direccion = "Test Address",
                Telefono = "123-456-7890",
                Email = "org@test.com",
                UsuarioId = 1
            };

            var opportunity = new VolunteerOpportunity
            {
                Id = 1,
                Titulo = "Test Volunteer Opportunity",
                Descripcion = "Help with community service",
                OrganizacionId = 1,
                Estatus = OpportunityStatus.Activa,
                FechaCreacion = DateTime.UtcNow,
                VoluntariosRequeridos = 5,
                VoluntariosInscritos = 0
            };

            // Create first volunteer badge
            var firstVolunteerBadge = new Badge
            {
                Id = 1,
                Name = "First Volunteer",
                Description = "Complete your first volunteer activity",
                IconPath = "/images/badges/first-volunteer.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 1,
                IsActive = true
            };

            _context.Organizaciones.Add(organization);
            _context.VolunteerOpportunities.Add(opportunity);
            _context.Badges.Add(firstVolunteerBadge);
            await _context.SaveChangesAsync();

            // Act 1: Volunteer applies to opportunity
            SetAuthorizationHeader(volunteerToken);
            
            var applyDto = new ApplyToOpportunityDto
            {
                OpportunityId = 1,
                Mensaje = "I'm excited to help with this opportunity!"
            };

            var applyContent = new StringContent(
                JsonSerializer.Serialize(applyDto),
                Encoding.UTF8,
                "application/json");

            var applyResponse = await _client.PostAsync("/api/voluntariado/apply", applyContent);
            applyResponse.EnsureSuccessStatusCode();

            // Act 2: Admin marks application as completed
            SetAuthorizationHeader(adminToken);

            var application = await _context.VolunteerApplications.FirstAsync();
            
            var updateStatusDto = new UpdateApplicationStatusDto
            {
                ApplicationId = application.Id,
                Status = ApplicationStatus.Completado,
                Notes = "Great work on the volunteer activity!"
            };

            var statusContent = new StringContent(
                JsonSerializer.Serialize(updateStatusDto),
                Encoding.UTF8,
                "application/json");

            var statusResponse = await _client.PutAsync("/api/voluntariado/application-status", statusContent);
            statusResponse.EnsureSuccessStatusCode();

            // Act 3: Check if badge was awarded (switch back to volunteer)
            SetAuthorizationHeader(volunteerToken);
            
            var badgesResponse = await _client.GetAsync("/api/badge/my-badges");
            badgesResponse.EnsureSuccessStatusCode();

            var badgesContent = await badgesResponse.Content.ReadAsStringAsync();
            var badgesResult = JsonSerializer.Deserialize<ApiResponseDto<IEnumerable<UserBadgeDto>>>(badgesContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Should have earned the first volunteer badge
            badgesResult.Should().NotBeNull();
            badgesResult!.Success.Should().BeTrue();
            badgesResult.Data.Should().NotBeNull();
            
            var earnedBadges = badgesResult.Data!.Where(b => b.IsEarned).ToList();
            earnedBadges.Should().HaveCount(1);
            earnedBadges.First().Name.Should().Be("First Volunteer");
            earnedBadges.First().EarnedDate.Should().NotBeNull();

            // Act 4: Check badge stats
            var statsResponse = await _client.GetAsync("/api/badge/stats");
            statsResponse.EnsureSuccessStatusCode();

            var statsContent = await statsResponse.Content.ReadAsStringAsync();
            var statsResult = JsonSerializer.Deserialize<ApiResponseDto<BadgeStatsDto>>(statsContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert: Stats should show one earned badge
            statsResult.Should().NotBeNull();
            statsResult!.Success.Should().BeTrue();
            statsResult.Data.Should().NotBeNull();
            statsResult.Data!.TotalEarned.Should().Be(1);
            statsResult.Data.ActivityBadges.Should().Be(1);
        }
    }
}