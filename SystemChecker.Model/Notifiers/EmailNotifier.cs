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
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Notifiers
{
    public class EmailNotifier : BaseNotifier
    {
        private enum Options
        {
            EmailAddresses = 2
        }
        private readonly ISettingsHelper _settingsHelper;
        public EmailNotifier(IRepository<CheckResult> checkResults, ISettingsHelper settingsHelper) : base(checkResults)
        {
            _settingsHelper = settingsHelper;
        }

        protected override async Task SendNotification(NotificationType type, string message, string failedAfter = null)
        {
            var settings = (await _settingsHelper.Get()).Global.Email;
            if (settings == null || string.IsNullOrEmpty(settings.From) || string.IsNullOrEmpty(settings.Server) || settings.Port == null)
            {
                throw new Exception("Email not set up");
            }
            List<int> emailIDs = _notification.Options[((int)Options.EmailAddresses).ToString()].ToObject<List<int>>();
            var emailAddresses = _settings.Contacts.Where(x => emailIDs.Contains(x.ID)).Select(x => x.Value);
            var subject = $"SystemChecker - {_check.Name}: {_result.Status.ToString()}";
            var from = settings.From;

            _logger.Info($"Sending email notification from {from} to {string.Join(", ", emailAddresses.ToArray())} with subject {subject}: {message}");

            _logger.Info($"Using SMTP server {settings.Server}:{settings.Port} with {(string.IsNullOrEmpty(settings.Username) ? "no username" : $" username {settings.Username}")} and {(settings.TLS ? "with" : "without")} TLS");

            var client = new SmtpClient(settings.Server, settings.Port.Value)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = settings.TLS
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
