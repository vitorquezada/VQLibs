using VQLib.Email.Model;
using VQLib.Util;

namespace VQLib.Email.Provider
{
    public abstract class VQBaseEmailService
    {
        public void ValidateEmail(VQEmail email)
        {
            if (email.Subject.IsNullOrWhiteSpace())
                throw new Exception("Subject not set");

            if (email.Body.IsNullOrWhiteSpace())
                throw new Exception("Body not set");

            if (email.To == null || !email.To.Any() || email.To.Any(x => x.Email.IsNullOrWhiteSpace()))
                throw new Exception("To without e-mail address");

            if (email.From == null || email.From.Email.IsNullOrWhiteSpace())
                throw new Exception("From without e-mail address");
        }
    }
}