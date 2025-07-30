using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using VoluntariadoConectadoRD.Controllers;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;
using Xunit;

namespace VoluntariadoConectadoRD.Tests
{
    public class ApplicationControllerTests
    {
        private readonly Mock<ILogger<VoluntariadoController>> _loggerMock;
        private readonly Mock<IOpportunityService> _opportunityServiceMock;
        private readonly VoluntariadoController _controller;

        public ApplicationControllerTests()
        {
            _loggerMock = new Mock<ILogger<VoluntariadoController>>();
            _opportunityServiceMock = new Mock<IOpportunityService>();
            _controller = new VoluntariadoController(_loggerMock.Object, _opportunityServiceMock.Object);
        }

        [Fact]
        public async Task GetMyApplications_ShouldReturnUserApplications_WhenValidUserId()
        {
            // Arrange
            var userId = 1;
            var expectedApplications = new List<ApplicationDto>
            {
                new ApplicationDto
                {
                    Id = 1,
                    OpportunityId = 1,
                    OpportunityTitulo = "Ayuda en Hospital",
                    UsuarioNombre = "Juan Pérez",
                    UsuarioEmail = "juan.perez@test.com",
                    Mensaje = "Me interesa mucho ayudar",
                    Estatus = ApplicationStatus.Pendiente,
                    FechaAplicacion = DateTime.UtcNow.AddDays(-5)
                },
                new ApplicationDto
                {
                    Id = 2,
                    OpportunityId = 2,
                    OpportunityTitulo = "Limpieza de Playa",
                    UsuarioNombre = "Juan Pérez",
                    UsuarioEmail = "juan.perez@test.com",
                    Mensaje = "Quiero contribuir al medio ambiente",
                    Estatus = ApplicationStatus.Aceptada,
                    FechaAplicacion = DateTime.UtcNow.AddDays(-3),
                    FechaRespuesta = DateTime.UtcNow.AddDays(-1)
                }
            };

            _opportunityServiceMock.Setup(x => x.GetUserApplicationsAsync(userId))
                .ReturnsAsync(expectedApplications);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.GetMyApplications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<IEnumerable<ApplicationDto>>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.Equal("Lista de mis postulaciones", response.Message);
            Assert.NotNull(response.Data);
            
            var applications = response.Data.ToList();
            Assert.Equal(2, applications.Count);
            Assert.Equal("Ayuda en Hospital", applications[0].OpportunityTitulo);
            Assert.Equal("Limpieza de Playa", applications[1].OpportunityTitulo);
        }

        [Fact]
        public async Task GetMyApplications_ShouldReturnBadRequest_WhenInvalidUserId()
        {
            // Arrange
            SetupUserClaims("invalid_user_id", "Voluntario");

            // Act
            var result = await _controller.GetMyApplications();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<IEnumerable<ApplicationDto>>>(badRequestResult.Value);
            
            Assert.False(response.Success);
            Assert.Equal("ID de usuario inválido", response.Message);
        }

        [Fact]
        public async Task GetMyApplications_ShouldReturnEmptyList_WhenUserHasNoApplications()
        {
            // Arrange
            var userId = 1;
            var emptyApplications = new List<ApplicationDto>();

            _opportunityServiceMock.Setup(x => x.GetUserApplicationsAsync(userId))
                .ReturnsAsync(emptyApplications);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.GetMyApplications();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<IEnumerable<ApplicationDto>>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetMyApplications_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var userId = 1;
            var exception = new Exception("Database connection failed");

            _opportunityServiceMock.Setup(x => x.GetUserApplicationsAsync(userId))
                .ThrowsAsync(exception);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.GetMyApplications();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            
            var response = Assert.IsType<ApiResponseDto<IEnumerable<ApplicationDto>>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Error interno del servidor", response.Message);
        }

        [Fact]
        public async Task ApplyToOpportunity_ShouldReturnSuccess_WhenApplicationIsCreated()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1;
            var applyDto = new ApplyToOpportunityDto
            {
                Mensaje = "Me interesa mucho esta oportunidad"
            };

            _opportunityServiceMock.Setup(x => x.ApplyToOpportunityAsync(opportunityId, userId, applyDto))
                .ReturnsAsync(true);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.ApplyToOpportunity(opportunityId, applyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<object>>(okResult.Value);
            
            Assert.True(response.Success);
            Assert.Equal("Aplicación enviada exitosamente", response.Message);
        }

        [Fact]
        public async Task ApplyToOpportunity_ShouldReturnBadRequest_WhenInvalidUserId()
        {
            // Arrange
            var opportunityId = 1;
            var applyDto = new ApplyToOpportunityDto
            {
                Mensaje = "Me interesa mucho esta oportunidad"
            };

            SetupUserClaims("invalid_user_id", "Voluntario");

            // Act
            var result = await _controller.ApplyToOpportunity(opportunityId, applyDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<object>>(badRequestResult.Value);
            
            Assert.False(response.Success);
            Assert.Equal("Usuario no válido", response.Message);
        }

        [Fact]
        public async Task ApplyToOpportunity_ShouldReturnBadRequest_WhenApplicationFails()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1;
            var applyDto = new ApplyToOpportunityDto
            {
                Mensaje = "Me interesa mucho esta oportunidad"
            };

            _opportunityServiceMock.Setup(x => x.ApplyToOpportunityAsync(opportunityId, userId, applyDto))
                .ReturnsAsync(false);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.ApplyToOpportunity(opportunityId, applyDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponseDto<object>>(badRequestResult.Value);
            
            Assert.False(response.Success);
            Assert.Equal("No se pudo enviar la aplicación. La oportunidad puede no existir o ya aplicaste anteriormente.", response.Message);
        }

        [Fact]
        public async Task ApplyToOpportunity_ShouldReturnInternalServerError_WhenServiceThrowsException()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1;
            var applyDto = new ApplyToOpportunityDto
            {
                Mensaje = "Me interesa mucho esta oportunidad"
            };
            var exception = new Exception("Database connection failed");

            _opportunityServiceMock.Setup(x => x.ApplyToOpportunityAsync(opportunityId, userId, applyDto))
                .ThrowsAsync(exception);

            SetupUserClaims(userId.ToString(), "Voluntario");

            // Act
            var result = await _controller.ApplyToOpportunity(opportunityId, applyDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            
            var response = Assert.IsType<ApiResponseDto<object>>(statusCodeResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Error interno del servidor", response.Message);
        }

        private void SetupUserClaims(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }
    }
} 