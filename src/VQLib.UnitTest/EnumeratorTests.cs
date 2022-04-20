using VQLib.UnitTest.Models.Enumerators;
using Xunit;

namespace VQLib.UnitTest
{
    public class EnumeratorTests
    {
        [Fact]
        public void IntegerEnumerator_SearchById_Test()
        {
            var test1Searched = TestIntegerEnumerator.GetById(1);
            Assert.Equal(test1Searched, TestIntegerEnumerator.Test1);
        }

        [Fact]
        public void IntegerEnumerator_Equal_Test()
        {
            var test1Searched = TestIntegerEnumerator.GetById(1);

            Assert.NotNull(test1Searched);

            Assert.True(TestIntegerEnumerator.Test1.Equals(test1Searched));

            Assert.True(TestIntegerEnumerator.Test1 == test1Searched);

            Assert.False(TestIntegerEnumerator.Test1 != test1Searched);
        }

        [Fact]
        public void IntegerEnumerator_GetByTerm_Test()
        {
            var searchResult = TestIntegerEnumerator.GetByDescriptionTerm("tes");

            Assert.Equal(6, searchResult.Count);
        }
    }
}