using System;
using System.Diagnostics;

namespace Test.Framework.Extended
{
	[DebuggerStepThrough]
	internal static class Assert
	{
#if XUNIT
		public static void Result(bool value, string message = "")
		{
			Xunit.Assert.True(!value, message);
		}
		public static void Fail(string message = "")
		{
			Xunit.Assert.True(false, message);
		}
		public static void Fail(Exception exception, bool stack = false)
		{
			var message = $"{exception.Message}{(stack ? $"\r\n\r\n{exception.StackTrace}" : "")}";
			Fail(message);
		}
		public static void Pass(string message = "")
		{
			Xunit.Assert.True(true, message);
		}
#else
        public static void Result(bool value, string message = "")
        {
            if (value) Pass(message);
            else Fail(message);
        }

        public static void Fail(string message = "")
        {
            NUnit.Framework.Assert.Fail(message);
        }

        public static void Fail(Exception exception, bool stack = false)
        {
            var message = $"{exception.Message}{(stack ? $"\r\n\r\n{exception.StackTrace}" : "")}";
            Fail(message);
        }

        public static void Pass(string message = "")
        {
            NUnit.Framework.Assert.Pass(message);
        }
#endif
	}
}
