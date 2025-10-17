using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Psychology.Application.Common.Interfaces;

namespace Psychology.Infrastructure.Email
{
    public sealed class EmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;

        public EmailSender(IOptions<EmailOptions> opt)
        {
            _opt = opt.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.FromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();

            var secure = ParseSecure(_opt.Security, _opt.Port);

            await smtp.ConnectAsync(_opt.Host, _opt.Port, secure, ct);

            if (!string.IsNullOrWhiteSpace(_opt.User))
            {
                await smtp.AuthenticateAsync(_opt.User, _opt.Pass, ct);
            }

            await smtp.SendAsync(msg, ct);
            await smtp.DisconnectAsync(true, ct);
        }

        private static SecureSocketOptions ParseSecure(string security, int port)
        {
            // Respect explicit setting first
            if (Enum.TryParse<SecureSocketOptions>(security, ignoreCase: true, out var parsed))
                return parsed;

            // Sensible defaults by port
            return port switch
            {
                465 => SecureSocketOptions.SslOnConnect, // SMTPS
                587 => SecureSocketOptions.StartTls,     // Submission
                _ => SecureSocketOptions.Auto
            };
        }
    }
}
