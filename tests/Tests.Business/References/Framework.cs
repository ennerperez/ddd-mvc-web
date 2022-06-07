#if XUNIT
namespace Xunit.Framework
{
    internal static class Assert
    {
        public static void Fail(string message = "")
        {
            Xunit.Assert.True(false, message);
        }

        public static void Pass(string message = "")
        {
            Xunit.Assert.True(true, message);
        }
    }
}
#elif SPECRUN
namespace SpecRunner.Framework
{
    internal static class Assert
    {
        public static void Fail(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(false, message);
        }
        public static void Pass(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true, message);
        }
    }
}
#endif
