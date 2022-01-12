using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using VQLib.Email.Enum;
using VQLib.Email.Model;
using VQLib.Util;

namespace VQLib.Email.Provider
{
    public class VQSendGridEmailService : IVQEmailService
    {
        public VQEmailProvider GetEmailProvider => VQEmailProvider.SendGrid;
        private string SendGridToken => _configuration.GetSection("SendGridConfig:Token").Value;

        private readonly IConfiguration _configuration;

        public VQSendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<VQSendEmailResult> SendEmail(VQEmail email)
        {
            if (email == null || email.From == null || !email.To.ListHasItem(x => !x.Email.IsNullOrWhiteSpace()))
            {
                throw new Exception("Email not found!");
            }

            var client = new SendGridClient(SendGridToken);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(email.From.Email, email.From.Name),
                Subject = email.Subject,
                HtmlContent = email.Body
            };
            foreach (var to in email.To)
            {
                if (to.Email.IsNullOrWhiteSpace())
                    continue;
                msg.AddTo(new EmailAddress(to.Email, to.Name));
            }
            foreach (var attachment in email.Attachments)
            {
                msg.AddAttachment(new Attachment()
                {
                    Content = Convert.ToBase64String(attachment.Content),
                    Type = attachment.Type,
                    Filename = attachment.Name
                });
            }

            var response = await client.SendEmailAsync(msg);

            return await ConvertResponseEmail(response);
        }

        private static async Task<VQSendEmailResult> ConvertResponseEmail(Response sendGridResponse)
        {
            var responseEmail = new VQSendEmailResult
            {
                Sucess = sendGridResponse.IsSuccessStatusCode,
                ErrorMessage = sendGridResponse.IsSuccessStatusCode ? string.Empty : await sendGridResponse.Body.ReadAsStringAsync()
            };
            return responseEmail;
        }
    }
}