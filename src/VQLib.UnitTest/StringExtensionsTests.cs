using System.IO;
using System.Text;
using System.Threading.Tasks;
using VQLib.Azure;
using VQLib.Azure.Storage.Blob;
using VQLib.Util;
using Xunit;

namespace VQLib.UnitTest
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("teste@test.com.br", "t***e@t**t.com.br")]
        [InlineData("teste@test.net", "t***e@t**t.net")]
        [InlineData("abc@abc.net", "a*c@a*c.net")]
        [InlineData("abc@abc.com.br.br", "a*c@a*c.com.br.br")]
        public void MaskedEmail_Test(string email, string maskedEmail)
        {
            Assert.Equal(maskedEmail, email.GetMaskedEmail());
        }

        [Fact]
        public async Task AzureBlobTest()
        {
            var configuracao = new VQAzureStorageConfig()
            {
                StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=vizeportal;AccountKey=xQyiGpT3omNr3vcz1hvWTgm32KkVpTZ++U4rCFBOmHP84B7lb8l681r+jrTZ94dwoKZ9Qgp9LGrn9025d/gPIw==;EndpointSuffix=core.windows.net"
            };
            var service = new VQAzureStorageService(configuracao);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Teste com sucesso"));
            var result = await service.Upload(stream, "test/test.txt");

            Assert.True(true);
        }
    }
}