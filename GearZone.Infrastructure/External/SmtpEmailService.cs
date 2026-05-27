using GearZone.Application.Abstractions.External;
using GearZone.Application.Common;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace GearZone.Infrastructure.External
{
    public class SmtpEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _username;
        private readonly string _password;

        public SmtpEmailService(IConfiguration configuration)
        {
            var emailSection = configuration.GetSection("Email");

            _host = emailSection["Host"]!;
            _port = int.Parse(emailSection["Port"]!);
            _senderName = emailSection["SenderName"]!;
            _senderEmail = configuration["EMAIL_SENDER_EMAIL"]!;
            _username = configuration["EMAIL_USERNAME"]!;
            _password = configuration["EMAIL_PASSWORD"]!;
        }
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendStoreStatusEmailAsync(Store store, StoreStatus status, string? reason)
        {
            if (store.OwnerUser == null || string.IsNullOrWhiteSpace(store.OwnerUser.Email))
                return;

            if (!Constant.subject.ContainsKey(status) || !Constant.body.ContainsKey(status))
                return;

            var subject = Constant.subject[status];
            var bodyTemplate = Constant.body[status];

            string body;

            switch (status)
            {
                case StoreStatus.Approved:
                    body = string.Format(
                        bodyTemplate,
                        store.OwnerUser.FullName,
                        store.StoreName
                    );
                    break;

                case StoreStatus.Rejected:
                    body = string.Format(
                        bodyTemplate,
                        store.OwnerUser.FullName,
                        store.StoreName,
                        reason
                    );
                    break;

                default:
                    return;
            }

            await SendAsync(store.OwnerUser.Email, subject, body);
        }
    }
}
