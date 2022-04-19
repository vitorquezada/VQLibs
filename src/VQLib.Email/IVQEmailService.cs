using System.Threading.Tasks;
using VQLib.Email.Enum;
using VQLib.Email.Model;

namespace VQLib.Email
{
    public interface IVQEmailService
    {
        VQEmailProvider GetEmailProvider { get; }

        Task<VQSendEmailResult> SendEmail(VQEmail email);
    }
}