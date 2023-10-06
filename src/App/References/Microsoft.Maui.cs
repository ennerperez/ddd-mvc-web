using Microsoft.Maui.Hosting;
namespace Microsoft.Maui
{
    public static class Extensions
    {
        public static MauiAppBuilder Close(this MauiAppBuilder @this)
        {
            return @this;
        }
    }
}
