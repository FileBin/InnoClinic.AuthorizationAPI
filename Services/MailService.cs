using System.Net;
using System.Net.Mail;

using Filebin.Domain.Auth.Abstraction.Services;
using Filebin.Common.Util;

namespace Filebin.AuthServer.Services;

public class MailService : IConfirmationMailService, IPasswordResetMailService {
    private readonly IConfiguration Configuration;
    public MailService(IConfiguration configuration) {
        Configuration = configuration;
    }

    public async Task SendConfirmationEmailAsync(string userEmail, string userId, string confirmationLink) {
        var mailText = $"Please confirm your E-Mail\n"
                     + $"{confirmationLink}";

        await SendEmailAsync(
           destination: userEmail,
           subject: "InnoShop email confirmation",
           message: mailText);
    }

    public async Task SendPasswordResetEmailAsync(string userEmail, string userId, string passwordResetLink) {
        var mailText = $"Please follow this link tor reset your password\n"
                     + $"{passwordResetLink}";

        await SendEmailAsync(
           destination: userEmail,
           subject: "InnoShop password reset",
           message: mailText);
    }

    public async Task SendEmailAsync(string destination, string subject, string message) {
        var conf = new {
            host = Configuration.GetOrThrow("SMTP:Host"),
            port = Convert.ToInt32(Configuration.GetOrThrow("SMTP:Port")),
            user = Configuration.GetOrThrow("SMTP:User"),
            password = Configuration.GetOrThrow("SMTP:Password"),
        };

        var client = new SmtpClient(conf.host, conf.port) {
            EnableSsl = true,
            Credentials = new NetworkCredential(conf.user, conf.password),
        };

        await client.SendMailAsync(new MailMessage(from: conf.user, to: destination, subject, message));
    }
}
