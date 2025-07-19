using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions
{
    public static class Extensions
    {
        public static ILoggingBuilder Close(this ILoggingBuilder @this)
        {
            return @this;
        }
    }

}
