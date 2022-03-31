// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace System.Net.Http
{
    public static class Methods
    {
        // This method must be in a class in a platform project, even if
        // the HttpClient object is constructed in a shared project.
        public static HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
#if DEBUG
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == Security.SslPolicyErrors.None;
#else
                return true;
#endif
            };
            return handler;
        }
    }
}
