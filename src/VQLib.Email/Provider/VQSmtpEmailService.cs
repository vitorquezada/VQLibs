using System.Net;
using System.Net.Mail;
using VQLib.Email.Enum;
using VQLib.Email.Model;
using VQLib.Util;

namespace VQLib.Email.Provider
{
    public class VQSmtpEmailService : VQBaseEmailService, IVQEmailService, IDisposable
    {
        private SmtpClient? _client = null;

        public VQEmailProvider GetEmailProvider => VQEmailProvider.SMTP;

        private VQSmtpConfig? _config { get; set; }

        private SmtpClient GetClient
        {
            get
            {
                if (_client == null)
                {
                    if (_config == null)
                        throw new ArgumentNullException(nameof(_config));

                    _client = new SmtpClient(_config.Host, _config.Port)
                    {
                        Credentials = new NetworkCredential(_config.UserName, _config.Password),
                        EnableSsl = _config.EnableSsl,
                    };
                }
                return _client;
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
        }

        public async Task<VQSendEmailResult> SendEmail(VQEmail email)
        {
            ValidateEmail(email);

            var msg = new MailMessage
            {
                Body = email.Body,
                IsBodyHtml = true,
                Subject = email.Subject,
                From = new MailAddress(email.From.Email, email.From.Name.IsNullOrWhiteSpaceOr(email.From.Email)),
            };

            if (email.To.ListHasItem())
                email.To.ForEach(x => msg.To.Add(new MailAddress(x.Email, x.Name.IsNullOrWhiteSpaceOr(x.Email))));

            if (email.Attachments.ListHasItem())
                email.Attachments.ForEach(x => msg.Attachments.Add(new Attachment(new MemoryStream(x.Content), x.Name, x.Type)));

            try
            {
                await GetClient.SendMailAsync(msg);
                return new VQSendEmailResult { Sucess = true };
            }
            catch (Exception ex)
            {
                return new VQSendEmailResult { Sucess = false, ErrorMessage = ex.Message };
            }
        }

        public void SetCredentials(string host, int port, bool useSsl, string userName, string password)
        {
            Dispose();

            _config = new VQSmtpConfig
            {
                Host = host,
                Port = port,
                EnableSsl = useSsl,
                UserName = userName,
                Password = password,
            };
        }

        public void SetCredentials(VQSmtpConfig model)
        {
            Dispose();

            _config = model;
        }
    }
}