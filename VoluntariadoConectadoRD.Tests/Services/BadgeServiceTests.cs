using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Tests.Services
{
    public class BadgeServiceTests : IDisposable
    {
        private readonly DbContextApplication _context;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<BadgeService>> _mockLogger;
        private readonly BadgeService _badgeService;

        public BadgeServiceTests()
        {
            var options = new DbContextOptionsBuilder<DbContextApplication>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DbContextApplication(options);
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<BadgeService>>();

            _badgeService = new BadgeService(_context, _mockNotificationService.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllBadgesAsync_ReturnsBadges()
        {
            // Arrange
            var badge1 = new Badge
            {
                Id = 1,
                Name = "First Volunteer",
                Description = "Complete your first volunteer activity",
                IconPath = "/images/badges/first-volunteer.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            var badge2 = new Badge
            {
                Id = 2,
                Name = "Dedicated Volunteer",
                Description = "Complete 10 volunteer activities",
                IconPath = "/images/badges/dedicated.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 10,
                IsActive = true
            };

            await _context.Badges.AddRangeAsync(badge1, badge2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _badgeService.GetAllBadgesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(b => b.Name == "First Volunteer");
            result.Should().Contain(b => b.Name == "Dedicated Volunteer");
        }

        [Fact]
        public async Task GetUserBadgesAsync_ReturnsUserBadgesWithEarnedDate()
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

            var badge = new Badge
            {
                Id = 1,
                Name = "First Volunteer",
                Description = "Complete your first volunteer activity",
                IconPath = "/images/badges/first-volunteer.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            var userBadge = new UserBadge
            {
                UserId = 1,
                BadgeId = 1,
                EarnedDate = DateTime.UtcNow.AddDays(-5)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddAsync(badge);
            await _context.UserBadges.AddAsync(userBadge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _badgeService.GetUserBadgesAsync(1);

            // Assert
            result.Should().HaveCount(1);
            var earnedBadge = result.First();
            earnedBadge.Name.Should().Be("First Volunteer");
            earnedBadge.EarnedDate.Should().NotBeNull();
            earnedBadge.IsEarned.Should().BeTrue();
        }

        [Fact]
        public async Task AwardBadgeAsync_NewBadge_AwardsSuccessfully()
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

            var badge = new Badge
            {
                Id = 1,
                Name = "First Volunteer",
                Description = "Complete your first volunteer activity",
                IconPath = "/images/badges/first-volunteer.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddAsync(badge);
            await _context.SaveChangesAsync();

            // Mock notification service
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()))
                .ReturnsAsync(new NotificationDto());

            // Act
            var result = await _badgeService.AwardBadgeAsync(1, 1);

            // Assert
            result.Should().BeTrue();

            var userBadge = await _context.UserBadges.FirstOrDefaultAsync(ub => ub.UserId == 1 && ub.BadgeId == 1);
            userBadge.Should().NotBeNull();
            userBadge!.EarnedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(It.Is<CreateNotificationDto>(n => 
                n.RecipientId == 1 && 
                n.Type == NotificationTypes.BADGE_EARNED && 
                n.Title == "¡Nueva insignia obtenida!")), Times.Once);
        }

        [Fact]
        public async Task AwardBadgeAsync_BadgeAlreadyEarned_ReturnsFalse()
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

            var badge = new Badge
            {
                Id = 1,
                Name = "First Volunteer",
                Description = "Complete your first volunteer activity",
                IconPath = "/images/badges/first-volunteer.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            var existingUserBadge = new UserBadge
            {
                UserId = 1,
                BadgeId = 1,
                EarnedDate = DateTime.UtcNow.AddDays(-1)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddAsync(badge);
            await _context.UserBadges.AddAsync(existingUserBadge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _badgeService.AwardBadgeAsync(1, 1);

            // Assert
            result.Should().BeFalse();

            // Verify no notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CheckAndAwardAutomaticBadgesAsync_FirstActivity_AwardsFirstVolunteerBadge()
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

            var application = new VolunteerApplication
            {
                Id = 1,
                UsuarioId = 1,
                OpportunityId = 1,
                Estatus = ApplicationStatus.Completado,
                FechaAplicacion = DateTime.UtcNow.AddDays(-1)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddAsync(firstVolunteerBadge);
            await _context.VolunteerApplications.AddAsync(application);
            await _context.SaveChangesAsync();

            // Mock notification service
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()))
                .ReturnsAsync(new NotificationDto());

            // Act
            await _badgeService.CheckAndAwardAutomaticBadgesAsync(1);

            // Assert
            var userBadge = await _context.UserBadges.FirstOrDefaultAsync(ub => ub.UserId == 1 && ub.BadgeId == 1);
            userBadge.Should().NotBeNull();

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(It.Is<CreateNotificationDto>(n => 
                n.Type == NotificationTypes.BADGE_EARNED)), Times.Once);
        }

        [Fact]
        public async Task CheckAndAwardAutomaticBadgesAsync_TenActivities_AwardsDedicatedVolunteerBadge()
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

            var dedicatedBadge = new Badge
            {
                Id = 2,
                Name = "Dedicated Volunteer",
                Description = "Complete 10 volunteer activities",
                IconPath = "/images/badges/dedicated.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 10,
                IsActive = true
            };

            // Create 10 completed applications
            var applications = new List<VolunteerApplication>();
            for (int i = 1; i <= 10; i++)
            {
                applications.Add(new VolunteerApplication
                {
                    Id = i,
                    UsuarioId = 1,
                    OpportunityId = i,
                    Estatus = ApplicationStatus.Completado,
                    FechaAplicacion = DateTime.UtcNow.AddDays(-i)
                });
            }

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddAsync(dedicatedBadge);
            await _context.VolunteerApplications.AddRangeAsync(applications);
            await _context.SaveChangesAsync();

            // Mock notification service
            _mockNotificationService.Setup(x => x.CreateNotificationAsync(It.IsAny<CreateNotificationDto>()))
                .ReturnsAsync(new NotificationDto());

            // Act
            await _badgeService.CheckAndAwardAutomaticBadgesAsync(1);

            // Assert
            var userBadge = await _context.UserBadges.FirstOrDefaultAsync(ub => ub.UserId == 1 && ub.BadgeId == 2);
            userBadge.Should().NotBeNull();

            // Verify notification was sent
            _mockNotificationService.Verify(x => x.CreateNotificationAsync(It.Is<CreateNotificationDto>(n => 
                n.Type == NotificationTypes.BADGE_EARNED)), Times.Once);
        }

        [Fact]
        public async Task GetUserBadgeStatsAsync_ReturnsCorrectStats()
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

            var badges = new List<Badge>
            {
                new() { Id = 1, Name = "Badge 1", Description = "Desc 1", IconPath = "/icon1.png", Type = BadgeType.Activity, IsActive = true },
                new() { Id = 2, Name = "Badge 2", Description = "Desc 2", IconPath = "/icon2.png", Type = BadgeType.Time, IsActive = true },
                new() { Id = 3, Name = "Badge 3", Description = "Desc 3", IconPath = "/icon3.png", Type = BadgeType.Leadership, IsActive = true }
            };

            var userBadges = new List<UserBadge>
            {
                new() { UserId = 1, BadgeId = 1, EarnedDate = DateTime.UtcNow.AddDays(-5) },
                new() { UserId = 1, BadgeId = 2, EarnedDate = DateTime.UtcNow.AddDays(-3) }
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Badges.AddRangeAsync(badges);
            await _context.UserBadges.AddRangeAsync(userBadges);
            await _context.SaveChangesAsync();

            // Act
            var result = await _badgeService.GetUserBadgeStatsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TotalEarned.Should().Be(2);
            result.TotalAvailable.Should().Be(3);
            result.ActivityBadges.Should().Be(1);
            result.TimeBadges.Should().Be(1);
            result.LeadershipBadges.Should().Be(0);
        }

        [Fact]
        public async Task CreateBadgeAsync_ValidBadge_CreatesSuccessfully()
        {
            // Arrange
            var createBadgeDto = new CreateBadgeDto
            {
                Name = "New Badge",
                Description = "A newly created badge",
                IconPath = "/images/badges/new.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 5
            };

            // Act
            var result = await _badgeService.CreateBadgeAsync(createBadgeDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createBadgeDto.Name);
            result.Description.Should().Be(createBadgeDto.Description);
            result.Type.Should().Be(createBadgeDto.Type);
            result.RequiredActivityCount.Should().Be(5);

            var savedBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "New Badge");
            savedBadge.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateBadgeAsync_ExistingBadge_UpdatesSuccessfully()
        {
            // Arrange
            var badge = new Badge
            {
                Id = 1,
                Name = "Original Badge",
                Description = "Original description",
                IconPath = "/original.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            await _context.Badges.AddAsync(badge);
            await _context.SaveChangesAsync();

            var updateBadgeDto = new UpdateBadgeDto
            {
                Name = "Updated Badge",
                Description = "Updated description",
                IconPath = "/updated.png",
                IsActive = false
            };

            // Act
            var result = await _badgeService.UpdateBadgeAsync(1, updateBadgeDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Badge");
            result.Description.Should().Be("Updated description");
            result.IconPath.Should().Be("/updated.png");
            result.IsActive.Should().BeFalse();

            var updatedBadge = await _context.Badges.FindAsync(1);
            updatedBadge!.Name.Should().Be("Updated Badge");
        }

        [Fact]
        public async Task DeleteBadgeAsync_ExistingBadge_DeletesSuccessfully()
        {
            // Arrange
            var badge = new Badge
            {
                Id = 1,
                Name = "Badge to Delete",
                Description = "This badge will be deleted",
                IconPath = "/delete.png",
                Type = BadgeType.Activity,
                IsActive = true
            };

            await _context.Badges.AddAsync(badge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _badgeService.DeleteBadgeAsync(1);

            // Assert
            result.Should().BeTrue();

            var deletedBadge = await _context.Badges.FindAsync(1);
            deletedBadge.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBadgeAsync_NonExistentBadge_ReturnsFalse()
        {
            // Act
            var result = await _badgeService.DeleteBadgeAsync(999);

            // Assert
            result.Should().BeFalse();
        }
    }
}