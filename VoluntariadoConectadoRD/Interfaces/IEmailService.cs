using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
        Task<bool> SendWelcomeEmailAsync(string email, string userName, string userType);
        Task<bool> SendVolunteerApplicationNotificationAsync(string organizationEmail, string volunteerName, string opportunityTitle);
        Task<bool> SendApplicationStatusUpdateAsync(string volunteerEmail, string opportunityTitle, string status, string message = "");
        Task<bool> SendOpportunityReminderAsync(string volunteerEmail, string opportunityTitle, DateTime eventDate);
        Task<bool> SendBulkNotificationAsync(List<string> emails, string subject, string message);
    }
}