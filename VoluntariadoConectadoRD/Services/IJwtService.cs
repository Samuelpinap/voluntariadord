using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Services
{
    public interface IJwtService
    {
        string GenerateToken(Usuario user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}