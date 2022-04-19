using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using VQLib.Email.Enum;
using VQLib.Email.Model;
using VQLib.Util;

namespace VQLib.Email.Provider
{
    public class VQSendGridEmailService : VQBaseEmailService, IVQEmailService
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
            ValidateEmail(email);

            var client = new SendGridClient(SendGridToken);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(email.From.Email, email.From.Name.IsNullOrWhiteSpaceOr(email.From.Email)),
                Subject = email.Subject,
                HtmlContent = email.Body
            };

            if (email.To.ListHasItem())
                email.To.ForEach(x => msg.AddTo(new EmailAddress(x.Email, x.Name.IsNullOrWhiteSpaceOr(x.Email))));

            if (email.Attachments.ListHasItem())
            {
                email.Attachments.ForEach(x => msg.AddAttachment(new Attachment
                {
                    Content = Convert.ToBase64String(x.Content),
                    Type = x.Type,
                    Filename = x.Name
                }));
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