using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ArchiveFqp.Services.Email
{
    public class EmailService
    {
        private readonly IReferenceDataService _refDataService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IReferenceDataService refDataService, IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _refDataService = refDataService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(int IdReceiver, string title, string message)
        {
            MimeMessage emailMessage = new();

            string server = _configuration["SmtpSettings:Server"]!;
            int port = int.Parse(_configuration["SmtpSettings:Port"]!);
            string senderName = _configuration["SmtpSettings:SenderName"]!;
            string senderEmail = _configuration["SmtpSettings:SenderEmail"]!;
            string username = _configuration["SmtpSettings:Username"]!;
            string password = _configuration["SmtpSettings:Password"]!;

            string? receiverEmail = (await _refDataService.GetAsync<Пользователь>())
                .FirstOrDefault(x => x.IdПользователя == IdReceiver)?.Email;
            if (string.IsNullOrWhiteSpace(receiverEmail)) return false;

            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            emailMessage.To.Add(new MailboxAddress("", receiverEmail));
            emailMessage.Subject = title;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using SmtpClient client = new();
            try
            {
                await client.ConnectAsync(server, port, true);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникла ошибка в отправке письма: {Message}", ex.Message);
            }

            return true;
        }
    }
}
