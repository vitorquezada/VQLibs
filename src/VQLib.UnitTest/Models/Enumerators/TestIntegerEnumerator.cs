using VQLib.Enumerator;

namespace VQLib.UnitTest.Models.Enumerators
{
    public class TestIntegerEnumerator : BaseEnumerator<TestIntegerEnumerator>
    {
        public static TestIntegerEnumerator Test1 = new(1, "Test1");
        public static TestIntegerEnumerator Test2 = new(2, "Test2");
        public static TestIntegerEnumerator Test3 = new(3, "Test3");
        public static TestIntegerEnumerator Test4 = new(4, "Test4");
        public static TestIntegerEnumerator Test5 = new(5, "Test5");
        public static TestIntegerEnumerator Test6 = new(6, "Test6");

        public TestIntegerEnumerator(int id, string description) : base(id, description)
        {
        }
    }
}