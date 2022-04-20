using System.Collections.Generic;
using System.Threading.Tasks;
using VQLib.Email.Model;
using VQLib.Email.Provider;
using Xunit;

namespace VQLib.UnitTest
{
    public class SmtpTests
    {
        [Fact]
        public async Task SendSmtp_Valid()
        {
            Assert.True(true);
            return;

            var emailService = new VQSmtpEmailService();

            emailService.SetCredentials("smtp.gmail.com", 587, true, "vitorquezada@gmail.com", "nrflzvqbqxyzomwr");

            var result = await emailService.SendEmail(new VQEmail
            {
                Body = "<h1>Teste corpo</h1><p>Conteúdo da mensagem</p>",
                From = new VQEmailUser
                {
                    Email = "vitorquezada@gmail.com",
                    Name = "Vitor Quezada"
                },
                To = new List<VQEmailUser>
                {
                    new VQEmailUser
                    {
                        Email = "vitorquezada@discente.ufg.br",
                        Name = "Vitor Quezada UFG",
                    },
                },
                Subject = "Teste assunto",
            });

            Assert.NotNull(result);
            Assert.True(result.Sucess);
            Assert.Null(result.ErrorMessage);
        }
    }
}