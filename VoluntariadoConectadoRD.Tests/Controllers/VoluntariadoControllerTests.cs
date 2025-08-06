using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using VoluntariadoConectadoRD.Controllers;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;

public class VoluntariadoControllerTests
{
    // ✅ Caso feliz: usuario autenticado y aplicación exitosa
    [Fact]
    public async Task ApplyToOpportunity_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var logger = new Mock<ILogger<VoluntariadoController>>();
        var service = new Mock<IOpportunityService>();

        int opportunityId = 1;
        int userId = 123;

        service.Setup(s => s.ApplyToOpportunityAsync(opportunityId, userId, null))
               .ReturnsAsync(true);

        var controller = new VoluntariadoController(logger.Object, service.Object);
        controller.ControllerContext = GetControllerContextWithUser(userId);

        // Act
        var result = await controller.ApplyToOpportunity(opportunityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Aplicación enviada exitosamente", response.Message);
    }

    // ❌ userId inválido (no viene en el token)
    [Fact]
    public async Task ApplyToOpportunity_ReturnsBadRequest_WhenUserIdMissing()
    {
        var logger = new Mock<ILogger<VoluntariadoController>>();
        var service = new Mock<IOpportunityService>();
        var controller = new VoluntariadoController(logger.Object, service.Object);

        // Usuario sin Claims
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.ApplyToOpportunity(1);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<object>>(badRequest.Value);
        Assert.False(response.Success);
    }

    // ❌ servicio devuelve false (fallo en la aplicación)
    [Fact]
    public async Task ApplyToOpportunity_ReturnsBadRequest_WhenServiceFails()
    {
        var logger = new Mock<ILogger<VoluntariadoController>>();
        var service = new Mock<IOpportunityService>();
        int opportunityId = 1;
        int userId = 456;

        service.Setup(s => s.ApplyToOpportunityAsync(opportunityId, userId, null))
               .ReturnsAsync(false);

        var controller = new VoluntariadoController(logger.Object, service.Object);
        controller.ControllerContext = GetControllerContextWithUser(userId);

        var result = await controller.ApplyToOpportunity(opportunityId);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<object>>(badRequest.Value);
        Assert.False(response.Success);
    }

    // ✅ Obtener mis aplicaciones devuelve el userId y rol correcto
    [Fact]
    public void GetMyApplications_ReturnsCorrectUserInfo()
    {
        var logger = new Mock<ILogger<VoluntariadoController>>();
        var service = new Mock<IOpportunityService>();

        var controller = new VoluntariadoController(logger.Object, service.Object);

        string userId = "999";
        string role = "Voluntario";

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role)
                }, "mock"))
            }
        };

        var result = controller.GetMyApplications();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<object>>(okResult.Value);
        Assert.True(response.Success);

        // Verifica que el userId en el resultado sea el mismo
        var data = response.Data!;
        var userIdProp = data.GetType().GetProperty("userId");
        var roleProp = data.GetType().GetProperty("userRole");

        Assert.Equal(userId, userIdProp?.GetValue(data)?.ToString());
        Assert.Equal(role, roleProp?.GetValue(data)?.ToString());
    }

    // 🔧 Función auxiliar para crear contexto con usuario autenticado
    private ControllerContext GetControllerContextWithUser(int userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
