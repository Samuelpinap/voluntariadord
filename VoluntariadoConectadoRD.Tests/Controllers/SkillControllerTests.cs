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
    public class SkillControllerTests
    {
        private readonly Mock<ISkillService> _mockSkillService;
        private readonly SkillController _controller;

        public SkillControllerTests()
        {
            _mockSkillService = new Mock<ISkillService>();
            _controller = new SkillController(_mockSkillService.Object);
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
        public async Task GetAllSkills_ReturnsOkWithSkills()
        {
            // Arrange
            var skills = new List<SkillDto>
            {
                new() { Id = 1, Name = "C# Programming", Description = "Object-oriented programming", CategoryId = 1, CategoryName = "Technical", IsActive = true },
                new() { Id = 2, Name = "JavaScript", Description = "Frontend programming", CategoryId = 1, CategoryName = "Technical", IsActive = true }
            };

            _mockSkillService.Setup(x => x.GetAllSkillsAsync())
                .ReturnsAsync(skills);

            // Act
            var result = await _controller.GetAllSkills();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<SkillDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSkillsByCategory_ValidCategory_ReturnsOkWithSkills()
        {
            // Arrange
            var skills = new List<SkillDto>
            {
                new() { Id = 1, Name = "C# Programming", Description = "Programming", CategoryId = 1, CategoryName = "Technical", IsActive = true }
            };

            _mockSkillService.Setup(x => x.GetSkillsByCategoryAsync(1))
                .ReturnsAsync(skills);

            // Act
            var result = await _controller.GetSkillsByCategory(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<SkillDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
            response.Data.First().CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task GetUserSkills_ValidUser_ReturnsOkWithUserSkills()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var userSkills = new List<UserSkillDto>
            {
                new() { SkillId = 1, SkillName = "C# Programming", Nivel = "Intermedio", CategoryName = "Technical", FechaObtencion = DateTime.UtcNow.AddDays(-30) },
                new() { SkillId = 2, SkillName = "JavaScript", Nivel = "Avanzado", CategoryName = "Technical", FechaObtencion = DateTime.UtcNow.AddDays(-60) }
            };

            _mockSkillService.Setup(x => x.GetUserSkillsAsync(1))
                .ReturnsAsync(userSkills);

            // Act
            var result = await _controller.GetUserSkills();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<UserSkillDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddSkillToUser_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var addSkillDto = new AddSkillToUserDto
            {
                SkillId = 1,
                Nivel = "Intermedio",
                Certificacion = "Online Certificate"
            };

            _mockSkillService.Setup(x => x.AddSkillToUserAsync(1, 1, "Intermedio", "Online Certificate"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddSkillToUser(addSkillDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Habilidad agregada exitosamente");
        }

        [Fact]
        public async Task AddSkillToUser_SkillAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var addSkillDto = new AddSkillToUserDto
            {
                SkillId = 1,
                Nivel = "Intermedio"
            };

            _mockSkillService.Setup(x => x.AddSkillToUserAsync(1, 1, "Intermedio", null))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddSkillToUser(addSkillDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("No se pudo agregar la habilidad. Es posible que ya la tengas.");
        }

        [Fact]
        public async Task UpdateUserSkill_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var updateDto = new UpdateUserSkillDto
            {
                Nivel = "Avanzado",
                Certificacion = "New Certificate"
            };

            _mockSkillService.Setup(x => x.UpdateUserSkillAsync(1, 1, updateDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUserSkill(1, updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Habilidad actualizada exitosamente");
        }

        [Fact]
        public async Task UpdateUserSkill_SkillNotFound_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var updateDto = new UpdateUserSkillDto
            {
                Nivel = "Avanzado"
            };

            _mockSkillService.Setup(x => x.UpdateUserSkillAsync(1, 999, updateDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUserSkill(999, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Habilidad no encontrada");
        }

        [Fact]
        public async Task RemoveSkillFromUser_ValidRequest_ReturnsOkWithSuccessMessage()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            _mockSkillService.Setup(x => x.RemoveSkillFromUserAsync(1, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveSkillFromUser(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Message.Should().Be("Habilidad eliminada exitosamente");
        }

        [Fact]
        public async Task RemoveSkillFromUser_SkillNotFound_ReturnsNotFound()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            _mockSkillService.Setup(x => x.RemoveSkillFromUserAsync(1, 999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RemoveSkillFromUser(999);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value as ApiResponseDto<string>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Habilidad no encontrada");
        }

        [Fact]
        public async Task GetAllSkillCategories_ReturnsOkWithCategories()
        {
            // Arrange
            var categories = new List<SkillCategoryDto>
            {
                new() { Id = 1, Name = "Technical Skills", Description = "Programming abilities" },
                new() { Id = 2, Name = "Soft Skills", Description = "Communication abilities" }
            };

            _mockSkillService.Setup(x => x.GetAllSkillCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAllSkillCategories();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<SkillCategoryDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserSkillStats_ValidUser_ReturnsOkWithStats()
        {
            // Arrange
            SetupUserContext(1, UserRole.Voluntario);
            
            var stats = new SkillStatsDto
            {
                TotalSkills = 5,
                CategoriesRepresented = 3,
                SkillsByCategory = new Dictionary<string, int>
                {
                    { "Technical", 3 },
                    { "Soft Skills", 2 }
                }
            };

            _mockSkillService.Setup(x => x.GetUserSkillStatsAsync(1))
                .ReturnsAsync(stats);

            // Act
            var result = await _controller.GetUserSkillStats();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<SkillStatsDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.TotalSkills.Should().Be(5);
            response.Data.CategoriesRepresented.Should().Be(3);
        }

        [Fact]
        public async Task CreateSkill_ValidSkill_ReturnsCreatedWithSkill()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var createDto = new CreateSkillDto
            {
                Name = "React.js",
                Description = "Frontend library",
                CategoryId = 1
            };

            var createdSkill = new SkillDto
            {
                Id = 1,
                Name = "React.js",
                Description = "Frontend library",
                CategoryId = 1,
                CategoryName = "Technical",
                IsActive = true
            };

            _mockSkillService.Setup(x => x.CreateSkillAsync(createDto))
                .ReturnsAsync(createdSkill);

            // Act
            var result = await _controller.CreateSkill(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var response = createdResult!.Value as ApiResponseDto<SkillDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.Name.Should().Be("React.js");
        }

        [Fact]
        public async Task CreateSkillCategory_ValidCategory_ReturnsCreatedWithCategory()
        {
            // Arrange
            SetupUserContext(1, UserRole.Administrador);
            
            var createDto = new CreateSkillCategoryDto
            {
                Name = "Design Skills",
                Description = "Visual design abilities"
            };

            var createdCategory = new SkillCategoryDto
            {
                Id = 1,
                Name = "Design Skills",
                Description = "Visual design abilities"
            };

            _mockSkillService.Setup(x => x.CreateSkillCategoryAsync(createDto))
                .ReturnsAsync(createdCategory);

            // Act
            var result = await _controller.CreateSkillCategory(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var response = createdResult!.Value as ApiResponseDto<SkillCategoryDto>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.Name.Should().Be("Design Skills");
        }

        [Fact]
        public async Task SearchSkills_ValidQuery_ReturnsOkWithMatchingSkills()
        {
            // Arrange
            var matchingSkills = new List<SkillDto>
            {
                new() { Id = 1, Name = "C# Programming", Description = "OOP programming", CategoryId = 1, CategoryName = "Technical", IsActive = true }
            };

            _mockSkillService.Setup(x => x.SearchSkillsAsync("Programming"))
                .ReturnsAsync(matchingSkills);

            // Act
            var result = await _controller.SearchSkills("Programming");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ApiResponseDto<IEnumerable<SkillDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().HaveCount(1);
            response.Data.First().Name.Should().Contain("Programming");
        }

        [Fact]
        public async Task SearchSkills_EmptyQuery_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SearchSkills("");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as ApiResponseDto<IEnumerable<SkillDto>>;
            
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Término de búsqueda requerido");
        }

        [Fact]
        public async Task GetAllSkills_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockSkillService.Setup(x => x.GetAllSkillsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllSkills();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            
            var response = objectResult.Value as ApiResponseDto<IEnumerable<SkillDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.Message.Should().Be("Error interno del servidor");
        }
    }
}