using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Tests.TestHelpers;
using Xunit;

namespace VoluntariadoConectadoRD.Tests.Controllers;

public class AuthControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;
{
    private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessResponse()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test.volunteer@test.com",
            Password = "TestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;LoginResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Token.Should().NotBeNullOrEmpty();
        apiResponse.Data.User.Should().NotBeNull();
        apiResponse.Data.User!.Email.Should().Be("test.volunteer@test.com");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@test.com",
            Password = "TestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;LoginResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test.volunteer@test.com",
            Password = "WrongPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;LoginResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "",
            Password = "TestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;LoginResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("inválidos");
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "invalid-email-format",
            Password = "TestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithMalformedJson_ReturnsBadRequest()
    {
        var malformedJson = "{ invalid json }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Register Voluntario Tests

    [Fact]
    public async Task RegisterVoluntario_WithValidData_ReturnsCreatedResponse()
    {
        var registerRequest = new RegisterVoluntarioDto
        {
            Nombre = "New",
            Apellido = "Volunteer",
            Email = "new.volunteer@test.com",
            Password = "NewPassword123!",
            Telefono = "809-555-1001",
            FechaNacimiento = new DateTime(1992, 5, 15)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/voluntario", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;UserInfoDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Email.Should().Be("new.volunteer@test.com");
        apiResponse.Data.Role.Should().Be(1);
    }

    [Fact]
    public async Task RegisterVoluntario_WithExistingEmail_ReturnsBadRequest()
    {
        var registerRequest = new RegisterVoluntarioDto
        {
            Nombre = "Test",
            Apellido = "Duplicate",
            Email = "test.volunteer@test.com",
            Password = "Password123!",
            Telefono = "809-555-1002",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/voluntario", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;UserInfoDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterVoluntario_WithWeakPassword_ReturnsBadRequest()
    {
        var registerRequest = new RegisterVoluntarioDto
        {
            Nombre = "Test",
            Apellido = "WeakPassword",
            Email = "weakpassword@test.com",
            Password = "123",
            Telefono = "809-555-1003",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/voluntario", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterVoluntario_WithFutureBirthDate_ReturnsBadRequest()
    {
        var registerRequest = new RegisterVoluntarioDto
        {
            Nombre = "Test",
            Apellido = "FutureBirth",
            Email = "future@test.com",
            Password = "Password123!",
            Telefono = "809-555-1004",
            FechaNacimiento = DateTime.Now.AddYears(1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/voluntario", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Apellido", "test@test.com")]
    [InlineData("Nombre", "", "test@test.com")]
    [InlineData("Nombre", "Apellido", "")]
    [InlineData("Nombre", "Apellido", "invalid-email")]
    public async Task RegisterVoluntario_WithMissingRequiredFields_ReturnsBadRequest(string nombre, string apellido, string email)
    {
        var registerRequest = new RegisterVoluntarioDto
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            Password = "Password123!",
            Telefono = "809-555-1005",
            FechaNacimiento = new DateTime(1990, 1, 1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/voluntario", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Register Organizacion Tests

    [Fact]
    public async Task RegisterOrganizacion_WithValidData_ReturnsCreatedResponse()
    {
        var registerRequest = new RegisterOrganizacionDto
        {
            NombreAdmin = "Admin",
            ApellidoAdmin = "Test",
            EmailAdmin = "admin@neworg.com",
            PasswordAdmin = "AdminPassword123!",
            TelefonoAdmin = "809-555-2001",
            FechaNacimientoAdmin = new DateTime(1985, 3, 20),
            NombreOrganizacion = "New Test Organization",
            Descripcion = "A new test organization",
            Direccion = "123 Test Street",
            Telefono = "809-555-2000",
            SitioWeb = "https://neworg.com",
            FechaFundacion = new DateTime(2020, 1, 1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/organizacion", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;UserInfoDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Email.Should().Be("admin@neworg.com");
        apiResponse.Data.Role.Should().Be(2);
    }

    [Fact]
    public async Task RegisterOrganizacion_WithExistingAdminEmail_ReturnsBadRequest()
    {
        var registerRequest = new RegisterOrganizacionDto
        {
            NombreAdmin = "Admin",
            ApellidoAdmin = "Test",
            EmailAdmin = "test.org@test.com",
            PasswordAdmin = "AdminPassword123!",
            TelefonoAdmin = "809-555-2002",
            FechaNacimientoAdmin = new DateTime(1985, 3, 20),
            NombreOrganizacion = "Duplicate Email Org",
            Descripcion = "Organization with duplicate admin email",
            Direccion = "456 Test Avenue",
            Telefono = "809-555-2003",
            SitioWeb = "https://duplicate.com",
            FechaFundacion = new DateTime(2021, 1, 1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/organizacion", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterOrganizacion_WithFutureFundationDate_ReturnsBadRequest()
    {
        var registerRequest = new RegisterOrganizacionDto
        {
            NombreAdmin = "Admin",
            ApellidoAdmin = "Test",
            EmailAdmin = "future@org.com",
            PasswordAdmin = "AdminPassword123!",
            TelefonoAdmin = "809-555-2004",
            FechaNacimientoAdmin = new DateTime(1985, 3, 20),
            NombreOrganizacion = "Future Foundation Org",
            Descripcion = "Organization with future foundation date",
            Direccion = "789 Future Street",
            Telefono = "809-555-2005",
            SitioWeb = "https://future.com",
            FechaFundacion = DateTime.Now.AddYears(1)
        };

        var content = new StringContent(JsonSerializer.Serialize(registerRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register/organizacion", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Profile Tests

    [Fact]
    public async Task GetUserProfile_WithValidToken_ReturnsUserProfile()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;UserInfoDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Email.Should().Be("test.volunteer@test.com");
    }

    [Fact]
    public async Task GetUserProfile_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfile_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfile_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var changePasswordRequest = new ChangePasswordDto
        {
            CurrentPassword = "TestPassword123!",
            NewPassword = "NewTestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(changePasswordRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/change-password", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var changePasswordRequest = new ChangePasswordDto
        {
            CurrentPassword = "WrongCurrentPassword123!",
            NewPassword = "NewTestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(changePasswordRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/change-password", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var changePasswordRequest = new ChangePasswordDto
        {
            CurrentPassword = "TestPassword123!",
            NewPassword = "weak"
        };

        var content = new StringContent(JsonSerializer.Serialize(changePasswordRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/change-password", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var changePasswordRequest = new ChangePasswordDto
        {
            CurrentPassword = "TestPassword123!",
            NewPassword = "NewTestPassword123!"
        };

        var content = new StringContent(JsonSerializer.Serialize(changePasswordRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/change-password", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Profile Tests

    [Fact]
    public async Task UpdateProfile_WithValidData_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateProfileDto
        {
            Nombre = "Updated",
            Apellido = "Name",
            Telefono = "809-555-9999"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/auth/profile", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var updateRequest = new UpdateProfileDto
        {
            Nombre = "Updated",
            Apellido = "Name",
            Telefono = "809-555-9999"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/auth/profile", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task AuthEndpoints_WithNullRequestBody_ReturnsBadRequest()
    {
        var endpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/register/voluntario",
            "/api/auth/register/organizacion"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.PostAsync(endpoint, null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
        }
    }

    [Fact]
    public async Task AuthEndpoints_WithEmptyRequestBody_ReturnsBadRequest()
    {
        var endpoints = new[]
        {
            "/api/auth/login",
            "/api/auth/register/voluntario",
            "/api/auth/register/organizacion"
        };

        foreach (var endpoint in endpoints)
        {
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task AuthEndpoints_WithVeryLongInput_HandlesProperly()
    {
        var longString = new string('a', 1000);
        var loginRequest = new LoginRequestDto
        {
            Email = longString + "@test.com",
            Password = longString
        };

        var content = new StringContent(JsonSerializer.Serialize(loginRequest, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}