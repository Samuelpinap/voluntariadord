using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VoluntariadoConectadoRD.Tests.TestHelpers;

public static class JwtTestHelper
{
    private const string TestSecretKey = "TestSecretKeyForJwtTokenGeneration12345678901234567890";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    public static string GenerateJwtToken(int userId, int role, string email, string nombre)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(TestSecretKey);
        
        var claims = new List&lt;Claim&gt;
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, nombre),
            new("UserId", userId.ToString()),
            new("Role", role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateVolunteerToken()
    {
        return GenerateJwtToken(1, 1, "test.volunteer@test.com", "Test Volunteer");
    }

    public static string GenerateOrganizationToken()
    {
        return GenerateJwtToken(2, 2, "test.org@test.com", "Test Organization Admin");
    }

    public static string GenerateAdminToken()
    {
        return GenerateJwtToken(3, 3, "test.admin@test.com", "Test Admin");
    }

    public static string GenerateExpiredToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(TestSecretKey);
        
        var claims = new List&lt;Claim&gt;
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Role, "1"),
            new(ClaimTypes.Email, "expired@test.com"),
            new(ClaimTypes.Name, "Expired User")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(-1),
            Issuer = TestIssuer,
            Audience = TestAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}