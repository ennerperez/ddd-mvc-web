using System;
using System.Diagnostics;

#if XUNIT
namespace Xunit.Framework
{
    internal static class Assert
    {
        [DebuggerStepThrough]
        public static void Result(bool value, string message = "")
        {
            Xunit.Assert.True(!value, message);
        }

        [DebuggerStepThrough]
        private static void Fail(string message = "")
        {
            Xunit.Assert.True(false, message);
        }

        [DebuggerStepThrough]
        public static void Fail(Exception exception, bool stack = false)
        {
            var message = $"{exception.Message}{(stack? $"\r\n\r\n{exception.StackTrace}" : "")}";
            Fail(message);
        }

        [DebuggerStepThrough]
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
        [DebuggerStepThrough]
        public static void Result(bool value, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(value, message);
        }

        [DebuggerStepThrough]
        public static void Fail(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(false, message);
        }

        [DebuggerStepThrough]
        public static void Pass(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true, message);
        }
    }
}
#endif
