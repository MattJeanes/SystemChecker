using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Interfaces;

namespace SystemChecker.Model.Notifiers
{
    public class EmailNotifier : BaseNotifier
    {
        private enum Options
        {
            EmailAddresses = 2
        }
        private readonly AppSettings _appSettings;
        public EmailNotifier(IRepository<CheckResult> checkResults, IOptions<AppSettings> appSettings) : base(checkResults)
        {
            _appSettings = appSettings.Value;
        }

        protected override async Task SendNotification(NotificationType type, string message, string failedAfter = null)
        {
            List<int> emailIDs = _notification.Options[((int)Options.EmailAddresses).ToString()].ToObject<List<int>>();
            var emailAddresses = _settings.Contacts.Where(x => emailIDs.Contains(x.ID)).Select(x => x.Value);
            var subject = $"SystemChecker - {_check.Name}: {_result.Status.ToString()}";
            var from = _appSettings.Email.From;

            _logger.Info($"Sending email notification from {from} to {string.Join(", ", emailAddresses.ToArray())} with subject {subject}: {message}");

            _logger.Info($"Using SMTP server { _appSettings.Email.Server}:{ _appSettings.Email.Port} with username { _appSettings.Email.Username} {(_appSettings.Email.TLS ? "with" : "without")} TLS");

            var client = new SmtpClient(_appSettings.Email.Server, _appSettings.Email.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_appSettings.Email.Username, _appSettings.Email.Password),
                EnableSsl = _appSettings.Email.TLS
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = message,
            };
            foreach (var emailAddress in emailAddresses)
            {
                mailMessage.To.Add(emailAddress);
            }

            await client.SendMailAsync(mailMessage);
        }
    }
}
