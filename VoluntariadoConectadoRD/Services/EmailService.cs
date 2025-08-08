using System.Net;
using System.Net.Mail;
using System.Text;
using VoluntariadoConectadoRD.Interfaces;

namespace VoluntariadoConectadoRD.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Configure SMTP client
            _smtpClient = new SmtpClient
            {
                Host = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? _configuration["EmailSettings:SmtpPort"] ?? "587"),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? _configuration["EmailSettings:SmtpUsername"],
                    Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _configuration["EmailSettings:SmtpPassword"]
                )
            };
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
        {
            try
            {
                var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? _configuration["BaseUrl"];
                var resetLink = $"{baseUrl}/Account/ResetPassword?token={resetToken}&email={email}";

                var subject = "Recuperación de Contraseña - Voluntariado Conectado RD";
                var body = GetPasswordResetEmailTemplate(userName, resetLink);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string userName, string userType)
        {
            try
            {
                var subject = "¡Bienvenido a Voluntariado Conectado RD!";
                var body = GetWelcomeEmailTemplate(userName, userType);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendVolunteerApplicationNotificationAsync(string organizationEmail, string volunteerName, string opportunityTitle)
        {
            try
            {
                var subject = $"Nueva Aplicación de Voluntario - {opportunityTitle}";
                var body = GetVolunteerApplicationNotificationTemplate(volunteerName, opportunityTitle);

                return await SendEmailAsync(organizationEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending volunteer application notification to {Email}", organizationEmail);
                return false;
            }
        }

        public async Task<bool> SendApplicationStatusUpdateAsync(string volunteerEmail, string opportunityTitle, string status, string message = "")
        {
            try
            {
                var subject = $"Actualización de tu Aplicación - {opportunityTitle}";
                var body = GetApplicationStatusUpdateTemplate(opportunityTitle, status, message);

                return await SendEmailAsync(volunteerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending application status update to {Email}", volunteerEmail);
                return false;
            }
        }

        public async Task<bool> SendOpportunityReminderAsync(string volunteerEmail, string opportunityTitle, DateTime eventDate)
        {
            try
            {
                var subject = $"Recordatorio: {opportunityTitle} - Mañana";
                var body = GetOpportunityReminderTemplate(opportunityTitle, eventDate);

                return await SendEmailAsync(volunteerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending opportunity reminder to {Email}", volunteerEmail);
                return false;
            }
        }

        public async Task<bool> SendBulkNotificationAsync(List<string> emails, string subject, string message)
        {
            try
            {
                var tasks = emails.Select(email => SendEmailAsync(email, subject, message));
                var results = await Task.WhenAll(tasks);
                
                var successCount = results.Count(r => r);
                _logger.LogInformation("Bulk email sent: {SuccessCount}/{TotalCount} successful", successCount, emails.Count);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") ?? _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "Voluntariado Conectado RD";

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(email);

                await _smtpClient.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                return false;
            }
        }

        private string GetPasswordResetEmailTemplate(string userName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #dc3545, #c82333); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; background: #dc3545; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Recuperación de Contraseña</h1>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userName}</strong>,</p>
                            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en Voluntariado Conectado RD.</p>
                            <p>Haz clic en el siguiente enlace para crear una nueva contraseña:</p>
                            <p><a href='{resetLink}' class='button'>Restablecer Contraseña</a></p>
                            <p>Si no solicitaste este cambio, puedes ignorar este correo de forma segura.</p>
                            <p><strong>Este enlace expirará en 24 horas.</strong></p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Voluntariado Conectado RD. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeEmailTemplate(string userName, string userType)
        {
            var welcomeMessage = userType.ToLower() == "voluntario" 
                ? "¡Te damos la bienvenida a nuestra comunidad de voluntarios!"
                : "¡Bienvenida tu organización a nuestra plataforma!";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #28a745, #20c997); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>¡Bienvenido/a!</h1>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{userName}</strong>,</p>
                            <p>{welcomeMessage}</p>
                            <p>Gracias por unirte a Voluntariado Conectado RD, la plataforma que conecta voluntarios con organizaciones en República Dominicana.</p>
                            <p>Ahora puedes:</p>
                            <ul>
                                <li>Explorar oportunidades de voluntariado</li>
                                <li>Conectar con organizaciones verificadas</li>
                                <li>Hacer la diferencia en tu comunidad</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Voluntariado Conectado RD. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetVolunteerApplicationNotificationTemplate(string volunteerName, string opportunityTitle)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #007bff, #0056b3); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Nueva Aplicación de Voluntario</h1>
                        </div>
                        <div class='content'>
                            <p><strong>{volunteerName}</strong> ha aplicado a la oportunidad: <strong>{opportunityTitle}</strong></p>
                            <p>Puedes revisar la aplicación en tu panel de administración.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetApplicationStatusUpdateTemplate(string opportunityTitle, string status, string message)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='content'>
                            <h2>Actualización de tu Aplicación</h2>
                            <p>Tu aplicación para <strong>{opportunityTitle}</strong> ha sido <strong>{status}</strong>.</p>
                            {(string.IsNullOrEmpty(message) ? "" : $"<p>Mensaje de la organización: {message}</p>")}
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetOpportunityReminderTemplate(string opportunityTitle, DateTime eventDate)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .content {{ background: #fff3cd; padding: 30px; border-radius: 10px; border-left: 4px solid #ffc107; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='content'>
                            <h2>🔔 Recordatorio de Evento</h2>
                            <p>Te recordamos que tienes el evento <strong>{opportunityTitle}</strong> programado para mañana.</p>
                            <p><strong>Fecha:</strong> {eventDate:dd/MM/yyyy HH:mm}</p>
                            <p>¡No olvides asistir! Tu participación es muy importante.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}