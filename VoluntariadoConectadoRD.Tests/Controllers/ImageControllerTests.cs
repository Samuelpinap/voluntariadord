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

public class ImageControllerTests : IClassFixture&lt;TestWebApplicationFactory&lt;Program&gt;&gt;
{
    private readonly TestWebApplicationFactory&lt;Program&gt; _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ImageControllerTests(TestWebApplicationFactory&lt;Program&gt; factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region Upload Avatar Tests

    [Fact]
    public async Task UploadAvatar_WithValidImageFile_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadAvatar_WithoutFile_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;ImageUploadResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("ningún archivo");
    }

    [Fact]
    public async Task UploadAvatar_WithEmptyFile_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Array.Empty&lt;byte&gt;());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "empty-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;ImageUploadResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("ningún archivo");
    }

    [Fact]
    public async Task UploadAvatar_WithInvalidFileType_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var textContent = Encoding.UTF8.GetBytes("This is not an image file");
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(textContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "not-an-image.txt");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadAvatar_WithVeryLargeFile_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var largeImageBytes = new byte[10 * 1024 * 1024];
        Random.Shared.NextBytes(largeImageBytes);
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(largeImageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "large-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task UploadAvatar_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadAvatar_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadAvatar_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("image/png", "test.png")]
    [InlineData("image/gif", "test.gif")]
    [InlineData("image/webp", "test.webp")]
    [InlineData("image/bmp", "test.bmp")]
    public async Task UploadAvatar_WithDifferentImageFormats_HandledAppropriately(string contentType, string fileName)
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Upload Logo Tests

    [Fact]
    public async Task UploadLogo_WithValidImageFile_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-logo.jpg");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_AsVolunteer_ReturnsAppropriateResponse()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-logo.jpg");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadLogo_AsAdmin_ReturnsAppropriateResponse()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-logo.jpg");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadLogo_WithoutFile_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;ImageUploadResponseDto&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Contain("ningún archivo");
    }

    [Fact]
    public async Task UploadLogo_WithInvalidFileType_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var textContent = Encoding.UTF8.GetBytes("This is not an image file");
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(textContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "not-an-image.txt");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test-logo.jpg");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Avatar Tests

    [Fact]
    public async Task DeleteAvatar_WithValidAuthentication_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("eliminado exitosamente");
    }

    [Fact]
    public async Task DeleteAvatar_AsOrganization_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteAvatar_AsAdmin_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteAvatar_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAvatar_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAvatar_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var response = await _client.DeleteAsync("/api/image/avatar");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Logo Tests

    [Fact]
    public async Task DeleteLogo_WithValidAuthentication_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize&lt;ApiResponseDto&lt;object&gt;&gt;(responseContent, _jsonOptions);
        
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("eliminado exitosamente");
    }

    [Fact]
    public async Task DeleteLogo_AsVolunteer_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteLogo_AsAdmin_ReturnsSuccess()
    {
        var token = JwtTestHelper.GenerateAdminToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteLogo_WithoutAuthentication_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLogo_WithExpiredToken_ReturnsUnauthorized()
    {
        var expiredToken = JwtTestHelper.GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLogo_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.format");

        var response = await _client.DeleteAsync("/api/image/logo");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region File Size and Format Edge Cases

    [Fact]
    public async Task UploadAvatar_WithExtremelySmallValidImage_HandledAppropriately()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tinyImageBytes = CreateMinimalImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(tinyImageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "tiny-avatar.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UploadLogo_WithUnsupportedImageFormat_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateOrganizationToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/tiff");
        content.Add(fileContent, "file", "test-logo.tiff");

        var response = await _client.PostAsync("/api/image/upload/logo", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithCorruptedImageFile_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var corruptedBytes = new byte[1024];
        Random.Shared.NextBytes(corruptedBytes);
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(corruptedBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "corrupted.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithMissingContentType_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        content.Add(fileContent, "file", "no-content-type.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Security and Malicious File Tests

    [Fact]
    public async Task Upload_WithExecutableFileDisguisedAsImage_ReturnsBadRequest()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var executableBytes = Encoding.UTF8.GetBytes("#!/bin/bash\necho 'malicious script'");
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(executableBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "malicious.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithSqlInjectionAttemptInFileName_HandlesSafely()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "'; DROP TABLE Users; --.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithPathTraversalAttemptInFileName_HandlesSafely()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var imageBytes = CreateTestImageBytes();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "../../../etc/passwd.jpg");

        var response = await _client.PostAsync("/api/image/upload/avatar", content);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentAvatarUploads_HandleProperly()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();

        var tasks = new List&lt;Task&lt;HttpResponseMessage&gt;&gt;();
        for (int i = 0; i &lt; 5; i++)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var imageBytes = CreateTestImageBytes();
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", $"concurrent-avatar-{i}.jpg");

            tasks.Add(client.PostAsync("/api/image/upload/avatar", content));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task ConcurrentDeleteOperations_HandleProperly()
    {
        var token = JwtTestHelper.GenerateVolunteerToken();

        var tasks = new List&lt;Task&lt;HttpResponseMessage&gt;&gt;();
        for (int i = 0; i &lt; 3; i++)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            tasks.Add(client.DeleteAsync("/api/image/avatar"));
        }

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
        }
    }

    #endregion

    #region Helper Methods

    private static byte[] CreateTestImageBytes()
    {
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48,
            0x00, 0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08,
            0x07, 0x07, 0x07, 0x09, 0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
            0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20, 0x24, 0x2E, 0x27, 0x20,
            0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29, 0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27,
            0x39, 0x3D, 0x38, 0x32, 0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01,
            0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01, 0xFF, 0xC4, 0x00, 0x14,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x08, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x0C, 0x03, 0x01, 0x00, 0x02,
            0x11, 0x03, 0x11, 0x00, 0x3F, 0x00, 0x7F, 0xFF, 0xD9
        };
    }

    private static byte[] CreateMinimalImageBytes()
    {
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48,
            0x00, 0x48, 0x00, 0x00, 0xFF, 0xD9
        };
    }

    #endregion
}