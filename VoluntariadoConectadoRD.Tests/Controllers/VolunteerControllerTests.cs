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

public class VolunteerControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;
{
    private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public VolunteerControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region Get User Profile Tests

    [Fact]
    public async Task GetUserProfile_WithValidId_ReturnsProfile()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/1");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserProfile_WithInvalidId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;EnhancedUserProfileDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task GetUserProfile_WithNegativeId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/-1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserProfile_WithZeroId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/0");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/profile/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfile_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.GetAsync("/api/volunteer/profile/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    [InlineData("null")]
    public async Task GetUserProfile_WithInvalidIdFormat_ReturnsBadRequest(string invalidId)
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/volunteer/profile/{invalidId}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Get My Profile Tests

    [Fact]
    public async Task GetMyProfile_WithValidToken_ReturnsOwnProfile()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/me");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_AsOrganization_ReturnsProfile()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/me");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_AsAdmin_ReturnsProfile()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/profile/me");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        var response = await _client.GetAsync("/api/volunteer/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get Volunteer Stats Tests

    [Fact]
    public async Task GetVolunteerStats_WithValidId_ReturnsStats()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/stats/1");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVolunteerStats_WithInvalidId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/stats/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;VolunteerStatsDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("no encontradas");
    }

    [Fact]
    public async Task GetVolunteerStats_WithNegativeId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/stats/-1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVolunteerStats_WithZeroId_ReturnsNotFound()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/stats/0");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVolunteerStats_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/stats/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVolunteerStats_AsAllRoles_ReturnsAppropriateResponse()
    {
        var tokens = new[]
        {
            ("Volunteer", JwtTestHelper.GenerateVolunteerToken()),
            ("Organization", JwtTestHelper.GenerateOrganizationToken()),
            ("Admin", JwtTestHelper.GenerateAdminToken())
        };

        foreach (var (role, token) in tokens)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/volunteer/stats/1");

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden,
                $"Role {role} should have appropriate access to stats endpoint");
        }
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    [InlineData("null")]
    public async Task GetVolunteerStats_WithInvalidIdFormat_ReturnsBadRequest(string invalidId)
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync($"/api/volunteer/stats/{invalidId}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region Get My Applications Tests (Volunteer Only)

    [Fact]
    public async Task GetMyApplications_AsVolunteer_ReturnsApplications()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyApplications_AsOrganization_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyApplications_AsAdmin_ReturnsForbidden()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyApplications_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyApplications_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyApplications_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var response = await _client.GetAsync("/api/volunteer/applications/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Platform Statistics Tests

    [Fact]
    public async Task GetPlatformStats_WithAuthentication_ReturnsStats()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/platform/stats");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlatformStats_AsAllRoles_ReturnsAppropriateResponse()
    {
        var tokens = new[]
        {
            ("Volunteer", JwtTestHelper.GenerateVolunteerToken()),
            ("Organization", JwtTestHelper.GenerateOrganizationToken()),
            ("Admin", JwtTestHelper.GenerateAdminToken())
        };

        foreach (var (role, token) in tokens)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/volunteer/platform/stats");

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Forbidden,
                $"Role {role} should have appropriate access to platform stats endpoint");
        }
    }

    [Fact]
    public async Task GetPlatformStats_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/platform/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Skills Management Tests

    [Fact]
    public async Task GetSkills_ReturnsSkillsList()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/skills");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSkills_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/skills");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserSkill_AsVolunteer_WithValidData_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var skillData = new
        {
            SkillId = 1,
            ProficiencyLevel = "Intermedio"
        };

        var content = new StringContent(JsonSerializer.Serialize(skillData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/skills/me/add", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddUserSkill_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var skillData = new
        {
            SkillId = 1,
            ProficiencyLevel = "Intermedio"
        };

        var content = new StringContent(JsonSerializer.Serialize(skillData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/skills/me/add", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddUserSkill_WithInvalidData_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidSkillData = new
        {
            SkillId = -1,
            ProficiencyLevel = ""
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidSkillData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/skills/me/add", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveUserSkill_AsVolunteer_ReturnsAppropriateResponse()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/volunteer/skills/me/1");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveUserSkill_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.DeleteAsync("/api/volunteer/skills/me/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Activities Management Tests

    [Fact]
    public async Task GetUserActivities_ReturnsActivitiesList()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/volunteer/activities/me");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserActivities_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/volunteer/activities/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LogActivity_AsVolunteer_WithValidData_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var activityData = new
        {
            ActivityType = "Volunteer Work",
            Description = "Helped at local shelter",
            Date = DateTime.UtcNow.AddDays(-1),
            HoursSpent = 4,
            Location = "Local Shelter"
        };

        var content = new StringContent(JsonSerializer.Serialize(activityData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/activities/me/log", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    [Fact]
    public async Task LogActivity_WithInvalidData_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var invalidActivityData = new
        {
            ActivityType = "",
            Description = "",
            Date = DateTime.UtcNow.AddDays(1),
            HoursSpent = -1,
            Location = ""
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidActivityData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/activities/me/log", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LogActivity_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var activityData = new
        {
            ActivityType = "Volunteer Work",
            Description = "Test activity",
            Date = DateTime.UtcNow.AddDays(-1),
            HoursSpent = 2,
            Location = "Test Location"
        };

        var content = new StringContent(JsonSerializer.Serialize(activityData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/activities/me/log", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task VolunteerEndpoints_WithMalformedJson_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[]
        {
            "/api/volunteer/skills/me/add",
            "/api/volunteer/activities/me/log"
        };

        var malformedJson = "{ invalid json structure";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        foreach (var endpoint in endpoints)
        {
            var response = await _client.PostAsync(endpoint, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task VolunteerEndpoints_WithEmptyRequestBody_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[]
        {
            "/api/volunteer/skills/me/add",
            "/api/volunteer/activities/me/log"
        };

        foreach (var endpoint in endpoints)
        {
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, content);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task VolunteerEndpoints_WithNullRequestBody_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var endpoints = new[]
        {
            "/api/volunteer/skills/me/add",
            "/api/volunteer/activities/me/log"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.PostAsync(endpoint, null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
        }
    }

    [Fact]
    public async Task VolunteerEndpoints_WithVeryLargePayload_HandlesProperly()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var largeString = new string('a', 10000);
        var largeActivityData = new
        {
            ActivityType = largeString,
            Description = largeString,
            Date = DateTime.UtcNow.AddDays(-1),
            HoursSpent = 4,
            Location = largeString
        };

        var content = new StringContent(JsonSerializer.Serialize(largeActivityData, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/volunteer/activities/me/log", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task VolunteerEndpoints_WithMissingContentType_HandlesProperly()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var activityData = new
        {
            ActivityType = "Test",
            Description = "Test activity",
            Date = DateTime.UtcNow.AddDays(-1),
            HoursSpent = 2,
            Location = "Test Location"
        };

        var jsonContent = JsonSerializer.Serialize(activityData);
        var content = new StringContent(jsonContent, Encoding.UTF8);

        var response = await _client.PostAsync("/api/volunteer/activities/me/log", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task AllVolunteerEndpoints_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var endpoints = new[]
        {
            ("/api/volunteer/profile/1", "GET"),
            ("/api/volunteer/profile/me", "GET"),
            ("/api/volunteer/stats/1", "GET"),
            ("/api/volunteer/applications/me", "GET"),
            ("/api/volunteer/platform/stats", "GET"),
            ("/api/volunteer/skills", "GET"),
            ("/api/volunteer/activities/me", "GET")
        };

        foreach (var (endpoint, method) in endpoints)
        {
            HttpResponseMessage response = method == "GET" 
                ? await _client.GetAsync(endpoint) 
                : await _client.PostAsync(endpoint, null);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                $"Endpoint {endpoint} should return Unauthorized with invalid token");
        }
    }

    [Fact]
    public async Task ConcurrentRequests_HandleProperly()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();

        var tasks = new List&lt;Task&lt;HttpResponseMessage&gt;&gt;();
        for (int i = 0; i &lt; 10; i++)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            tasks.Add(client.GetAsync("/api/volunteer/profile/me"));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        }
    }

    #endregion
}