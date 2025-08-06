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

public class VoluntariadoControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;
{
    private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public VoluntariadoControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region Get Opportunities Tests (Public Endpoint)

    [Fact]
    public async Task GetVolunteerOpportunities_WithoutAuthentication_ReturnsOpportunities()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/opportunities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;IEnumerable&lt;OpportunityListDto&gt;&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Message.Should().Contain("oportunidades");
    }

    [Fact]
    public async Task GetVolunteerOpportunities_WithAuthentication_ReturnsOpportunities()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/opportunities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;IEnumerable&lt;OpportunityListDto&gt;&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
    }

    #endregion

    #region Get Opportunity Details Tests (Public Endpoint)

    [Fact]
    public async Task GetOpportunityDetails_WithValidId_ReturnsOpportunityDetails()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/opportunities/1");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOpportunityDetails_WithInvalidId_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/opportunities/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;OpportunityDetailDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no encontrada");
    }

    [Fact]
    public async Task GetOpportunityDetails_WithNegativeId_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/opportunities/-1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOpportunityDetails_WithZeroId_ReturnsNotFound()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/opportunities/0");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Apply to Opportunity Tests (Volunteer Only)

    [Fact]
    public async Task ApplyToOpportunity_AsVolunteer_WithValidId_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var applyDto = new ApplyToOpportunityDto
        {
            Motivation = "I am very motivated to participate in this opportunity",
            Experience = "Previous volunteer experience in similar activities"
        };

        var content = new StringContent(JsonSerializer.Serialize(applyDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/apply/1", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyToOpportunity_AsVolunteer_WithoutBody_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/voluntariado/apply/1", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyToOpportunity_AsOrganization_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/voluntariado/apply/1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApplyToOpportunity_AsAdmin_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/voluntariado/apply/1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApplyToOpportunity_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsync("/api/voluntariado/apply/1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApplyToOpportunity_WithInvalidOpportunityId_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsync("/api/voluntariado/apply/99999", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyToOpportunity_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.PostAsync("/api/voluntariado/apply/1", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Create Opportunity Tests (Organization Only)

    [Fact]
    public async Task CreateOpportunity_AsOrganization_WithValidData_ReturnsCreated()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateOpportunityDto
        {
            Title = "New Test Opportunity",
            Description = "This is a new test opportunity for volunteers",
            RequiredSkills = "Communication, Teamwork",
            Location = "Santo Domingo, DR",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(17),
            MaxVolunteers = 15
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateOpportunity_AsVolunteer_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateOpportunityDto
        {
            Title = "Volunteer Created Opportunity",
            Description = "This should not be allowed",
            RequiredSkills = "None",
            Location = "Test Location",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            MaxVolunteers = 5
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOpportunity_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var createDto = new CreateOpportunityDto
        {
            Title = "Unauthorized Opportunity",
            Description = "This should not be allowed",
            RequiredSkills = "None",
            Location = "Test Location",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            MaxVolunteers = 5
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOpportunity_WithInvalidDates_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateOpportunityDto
        {
            Title = "Invalid Date Opportunity",
            Description = "This has invalid dates",
            RequiredSkills = "None",
            Location = "Test Location",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(5),
            MaxVolunteers = 5
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Description", "Skills", "Location", 5)]
    [InlineData("Title", "", "Skills", "Location", 5)]
    [InlineData("Title", "Description", "", "Location", 5)]
    [InlineData("Title", "Description", "Skills", "", 5)]
    [InlineData("Title", "Description", "Skills", "Location", 0)]
    [InlineData("Title", "Description", "Skills", "Location", -1)]
    public async Task CreateOpportunity_WithMissingRequiredFields_ReturnsBadRequest(
        string title, string description, string skills, string location, int maxVolunteers)
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateOpportunityDto
        {
            Title = title,
            Description = description,
            RequiredSkills = skills,
            Location = location,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            MaxVolunteers = maxVolunteers
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Get Applications Tests (Organization Only)

    [Fact]
    public async Task GetApplications_AsOrganization_ReturnsApplicationsList()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("aplicaciones");
    }

    [Fact]
    public async Task GetApplications_AsVolunteer_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/applications");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetApplications_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/applications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Admin Stats Tests (Organization and Admin Only)

    [Fact]
    public async Task GetAdminStats_AsOrganization_ReturnsStats()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("Estadísticas");
    }

    [Fact]
    public async Task GetAdminStats_AsAdmin_ReturnsStats()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetAdminStats_AsVolunteer_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminStats_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get My Applications Tests (Volunteer and Admin Only)

    [Fact]
    public async Task GetMyApplications_AsVolunteer_ReturnsApplications()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/my-applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("aplicaciones");
    }

    [Fact]
    public async Task GetMyApplications_AsAdmin_ReturnsApplications()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/my-applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetMyApplications_AsOrganization_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/voluntariado/my-applications");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyApplications_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/voluntariado/my-applications");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Update Opportunity Tests (Organization and Admin Only)

    [Fact]
    public async Task UpdateOpportunity_AsOrganization_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateData = new
        {
            Title = "Updated Opportunity Title",
            Description = "Updated description",
            MaxVolunteers = 20
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/voluntariado/opportunities/1", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("actualizada");
    }

    [Fact]
    public async Task UpdateOpportunity_AsAdmin_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateData = new
        {
            Title = "Admin Updated Opportunity",
            Status = "Cerrada"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/voluntariado/opportunities/1", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOpportunity_AsVolunteer_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateData = new
        {
            Title = "Volunteer Should Not Update"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/voluntariado/opportunities/1", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateOpportunity_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var updateData = new
        {
            Title = "Unauthorized Update"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/voluntariado/opportunities/1", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateOpportunity_WithInvalidId_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateData = new
        {
            Title = "Update Non-Existent Opportunity"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/voluntariado/opportunities/99999", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Theory]
    [InlineData("/api/voluntariado/opportunities/abc")]
    [InlineData("/api/voluntariado/opportunities/1.5")]
    [InlineData("/api/voluntariado/opportunities/")]
    public async Task OpportunityEndpoints_WithInvalidIdFormats_ReturnsBadRequest(string endpoint)
    {
        var response = await _client.GetAsync(endpoint);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOpportunity_WithMalformedJson_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var malformedJson = "{ invalid json structure";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyToOpportunity_WithMalformedJson_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var malformedJson = "{ incomplete json";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/apply/1", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EndpointsWithAuthentication_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var endpoints = new[]
        {
            "/api/voluntariado/apply/1",
            "/api/voluntariado/opportunities",
            "/api/voluntariado/applications",
            "/api/voluntariado/admin/stats",
            "/api/voluntariado/my-applications"
        };

        foreach (var endpoint in endpoints)
        {
            HttpResponseMessage response;
            if (endpoint.Contains("apply") || endpoint.Contains("opportunities") &amp;&amp; !endpoint.Contains("admin"))
            {
                response = await _client.PostAsync(endpoint, null);
            }
            else
            {
                response = await _client.GetAsync(endpoint);
            }
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task CreateOpportunity_WithVeryLongInput_HandlesProperly()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var longString = new string('a', 5000);
        var createDto = new CreateOpportunityDto
        {
            Title = longString,
            Description = longString,
            RequiredSkills = longString,
            Location = longString,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(10),
            MaxVolunteers = 5
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task Endpoints_WithMissingContentType_HandlesProperly()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var jsonContent = JsonSerializer.Serialize(new { Title = "Test" });
        var content = new StringContent(jsonContent, Encoding.UTF8);

        var response = await _client.PostAsync("/api/voluntariado/opportunities", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    #endregion
}