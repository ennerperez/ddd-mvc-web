namespace System.Net.Http
{
    public static class Methods
    {
        /// <summary>
        /// This method must be in a class in a platform project, even if the HttpClient object is constructed in a shared project.
        /// </summary>
        /// <returns></returns>
        public static HttpClientHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
#if DEBUG
                    if (cert.Issuer.Equals("CN=localhost"))
                    {
                        return true;
                    }

                    return errors == Security.SslPolicyErrors.None;
#else
                    return true;
#endif
                }
            };
            return handler;
        }
    }
}
