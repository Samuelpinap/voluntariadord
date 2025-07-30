using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Usuario user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}