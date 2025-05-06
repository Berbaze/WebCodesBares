using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace WebCodesBares.Data.Service
{
    public class EmailService : IEmailSender
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailSettings = config.GetSection("EmailSettings").Get<EmailSettings>()
                ?? throw new ArgumentNullException(nameof(_emailSettings), "Les paramètres EmailSettings sont manquants.");

            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) ||
                string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_emailSettings.Username) ||
                string.IsNullOrWhiteSpace(_emailSettings.Password))
            {
                throw new InvalidOperationException("⚠️ Les paramètres SMTP sont incomplets.");
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("⚠️ Tentative d'envoi d'email avec une adresse vide !");
                return;
            }

            try
            {
                using var client = CreateSmtpClient();
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true // toujours HTML pour flexibilité
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("✅ Email envoyé à {Email} avec le sujet : {Subject}", email, subject);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError("❌ SMTP ERREUR {Status} - {Message}", smtpEx.StatusCode, smtpEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ ERREUR en envoyant à {Email} : {Message}", email, ex.Message);
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15000
            };
        }
    }
}
