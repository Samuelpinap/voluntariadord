namespace VoluntariadoConectadoRD.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        bool IsPasswordStrong(string password);
    }
}