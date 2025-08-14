using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using VoluntariadoConectadoRD.Controllers;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace VoluntariadoConectadoRD.Tests.Controllers
{
    public class BadgeControllerTests
    {
        private readonly Mock<IBadgeService> _mockBadgeService;
        private readonly BadgeController _controller;

        public BadgeControllerTests()
        {
            _mockBadgeService = new Mock<IBadgeService>();
            _controller = new BadgeController(_mockBadgeService.Object);
        }

        private void SetupUserContext(int userId, UserRole role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, ((int)role).ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetAllBadges_ReturnsOkWithBadges()
        {
            // Arrange
            var badges = new List<BadgeDto>
            {
                new() { Id = 1, Name = "First Volunteer", Description = "Complete first activity", IsActive = true },
                new() { Id = 2, Name = "Dedicated", Description = "Complete 10 activities", IsActive = true }
            };

            _mockBadgeService.Setup(x => x.GetAllBadgesAsync())
                .ReturnsAsync(badges);

            // Act
            var result = await _controller.GetAllBadges();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<BadgeDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserBadges_ValidUser_ReturnsOkWithUserBadges()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var userBadges = new List<UserBadgeDto>
            {
                new() { Id = 1, Name = "First Volunteer", IsEarned = true, EarnedDate = DateTime.UtcNow.AddDays(-5) },
                new() { Id = 2, Name = "Dedicated", IsEarned = false, EarnedDate = null }
            };

            _mockBadgeService.Setup(x => x.GetUserBadgesAsync(1))
                .ReturnsAsync(userBadges);

            // Act
            var result = await _controller.GetUserBadges();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<UserBadgeDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data.Should().Contain(b => b.IsEarned);
            response.Data.Should().Contain(b => !b.IsEarned);
        }

        [Fact]
        public async Task AwardBadge_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var awardDto = new AwardBadgeDto { UserId = 2, BadgeId = 1 };
            
            _mockBadgeService.Setup(x => x.AwardBadgeAsync(2, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AwardBadge(awardDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Insignia otorgada exitosamente");
        }

        [Fact]
        public async Task AwardBadge_BadgeAlreadyEarned_ReturnsBadRequestWithErrorMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var awardDto = new AwardBadgeDto { UserId = 2, BadgeId = 1 };
            
            _mockBadgeService.Setup(x => x.AwardBadgeAsync(2, 1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AwardBadge(awardDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("No se pudo otorgar la insignia. Es posible que el usuario ya la tenga.");
        }

        [Fact]
        public async Task GetUserBadgeStats_ValidUser_ReturnsOkWithStats()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var stats = new BadgeStatsDto
            {
                TotalEarned = 3,
                TotalAvailable = 10,
                ActivityBadges = 2,
                TimeBadges = 1,
                LeadershipBadges = 0
            };

            _mockBadgeService.Setup(x => x.GetUserBadgeStatsAsync(1))
                .ReturnsAsync(stats);

            // Act
            var result = await _controller.GetUserBadgeStats();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<BadgeStatsDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.TotalEarned.Should().Be(3);
            response.Data.TotalAvailable.Should().Be(10);
        }

        [Fact]
        public async Task CreateBadge_ValidBadge_ReturnsCreatedWithBadge()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var createDto = new CreateBadgeDto
            {
                Name = "New Badge",
                Description = "A newly created badge",
                IconPath = "/images/badges/new.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 5
            };

            var createdBadge = new BadgeDto
            {
                Id = 1,
                Name = "New Badge",
                Description = "A newly created badge",
                IconPath = "/images/badges/new.png",
                Type = BadgeType.Activity,
                RequiredActivityCount = 5,
                IsActive = true
            };

            _mockBadgeService.Setup(x => x.CreateBadgeAsync(createDto))
                .ReturnsAsync(createdBadge);

            // Act
            var result = await _controller.CreateBadge(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var response = createdResult!.Value as ApiResponseDto<BadgeDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.Name.Should().Be("New Badge");
        }

        [Fact]
        public async Task UpdateBadge_ValidBadge_ReturnsOkWithUpdatedBadge()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var updateDto = new UpdateBadgeDto
            {
                Name = "Updated Badge",
                Description = "Updated description",
                IsActive = false
            };

            var updatedBadge = new BadgeDto
            {
                Id = 1,
                Name = "Updated Badge",
                Description = "Updated description",
                IsActive = false
            };

            _mockBadgeService.Setup(x => x.UpdateBadgeAsync(1, updateDto))
                .ReturnsAsync(updatedBadge);

            // Act
            var result = await _controller.UpdateBadge(1, updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<BadgeDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data!.Name.Should().Be("Updated Badge");
            response.Data.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateBadge_BadgeNotFound_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var updateDto = new UpdateBadgeDto
            {
                Name = "Updated Badge",
                Description = "Updated description"
            };

            _mockBadgeService.Setup(x => x.UpdateBadgeAsync(999, updateDto))
                .ReturnsAsync((BadgeDto?)null);

            // Act
            var result = await _controller.UpdateBadge(999, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value as ApiResponseDto<BadgeDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Insignia no encontrada");
        }

        [Fact]
        public async Task DeleteBadge_ValidBadge_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            _mockBadgeService.Setup(x => x.DeleteBadgeAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBadge(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Insignia eliminada exitosamente");
        }

        [Fact]
        public async Task DeleteBadge_BadgeNotFound_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            _mockBadgeService.Setup(x => x.DeleteBadgeAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBadge(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Insignia no encontrada");
        }

        [Fact]
        public async Task GetAllBadges_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockBadgeService.Setup(x => x.GetAllBadgesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllBadges();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            
            var response = objectResult.Value as ApiResponseDto<IEnumerable<BadgeDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Error interno del servidor");
        }
    }
}