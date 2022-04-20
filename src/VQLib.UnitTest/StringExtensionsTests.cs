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

        [Theory]
        [InlineData("Tésté", "Teste")]
        public void RemoveAccents_Test(string withDiatricts, string withoutDiatricts)
        {
            var str = withDiatricts.RemoveAccents();
            Assert.Equal(str, withoutDiatricts);
        }

        [Theory]
        [InlineData("Tésté", "Tes")]
        [InlineData("Tésté", "tes")]
        [InlineData("Tésté", "ST")]
        public void ContainsIgnoreCaseAndAccents_Test(string x1, string x2)
        {
            Assert.True(x1.ContainsIgnoreCaseAndAccents(x2));
        }
    }
}