using System.Net;
using System.Net.Mail;
using LMS.Services.Interfaces;

namespace LMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string code)
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var senderEmail = _configuration["Smtp:SenderEmail"] ?? "luxury010100@gmail.com";
            var senderPassword = _configuration["Smtp:SenderPassword"] ?? "yttoovmxoqpislvq";

            var subject = "[LMS Learning System] Your Password Reset Verification Code";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 550px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #4f46e5; margin: 0;'>LMS Learning System</h2>
                        <p style='color: #64748b; font-size: 14px;'>Password Reset Request Verification</p>
                    </div>
                    <div style='background-color: #f8fafc; padding: 20px; border-radius: 10px; text-align: center; margin-bottom: 20px;'>
                        <p style='margin: 0 0 10px 0; color: #334155; font-size: 15px;'>Your 6-digit verification OTP code is:</p>
                        <h1 style='color: #4f46e5; font-size: 36px; letter-spacing: 6px; margin: 10px 0; font-family: monospace;'>{code}</h1>
                        <p style='margin: 10px 0 0 0; color: #ef4444; font-size: 13px;'>This code is valid for 15 minutes.</p>
                    </div>
                    <p style='color: #64748b; font-size: 13px;'>If you did not request a password reset, please ignore this email.</p>
                </div>";

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "LMS Learning System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Password reset code email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            }
        }
    }
}
