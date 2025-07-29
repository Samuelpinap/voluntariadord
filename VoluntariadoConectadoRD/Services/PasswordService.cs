using BCrypt.Net;
using System.Text.RegularExpressions;
using VoluntariadoConectadoRD.Interfaces;

namespace VoluntariadoConectadoRD.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            // Debe tener al menos una letra minúscula
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Debe tener al menos una letra mayúscula
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Debe tener al menos un número
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            // Debe tener al menos un carácter especial
            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
                return false;

            return true;
        }
    }
}