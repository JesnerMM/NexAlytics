using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NexAlytics.Application.Settings;

namespace NexAlytics.Application.Services;

public class EmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("SMTP host not configured. Skipping password reset email to {Email}. Reset link: {Link}", toEmail, resetLink);
            return;
        }

        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 24px;">
              <div style="max-width: 480px; margin: auto; background: #fff; border-radius: 8px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                <h2 style="color: #3b82f6; margin-bottom: 8px;">NEXAlytics</h2>
                <h3 style="color: #1e293b;">Recuperación de contraseña</h3>
                <p>Hola <strong>{toName}</strong>,</p>
                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta. Haz clic en el enlace de abajo para crear una nueva contraseña:</p>
                <p style="text-align: center; margin: 28px 0;">
                  <a href="{resetLink}" style="background: #3b82f6; color: #fff; padding: 12px 28px; border-radius: 6px; text-decoration: none; font-weight: bold;">
                    Restablecer contraseña
                  </a>
                </p>
                <p style="color: #64748b; font-size: 13px;">Este enlace expirará en <strong>1 hora</strong>. Si no solicitaste el restablecimiento de contraseña, puedes ignorar este mensaje.</p>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;" />
                <p style="color: #94a3b8; font-size: 12px;">NEXAlytics — Plataforma de Analítica Empresarial</p>
              </div>
            </body>
            </html>
            """;

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            Subject = "Recuperación de contraseña — NEXAlytics",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail, toName));

        await client.SendMailAsync(message);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
