using Infrastructure.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web.Services
{
    public class SmtpSender : SmtpService, IEmailSender
    {
        public SmtpSender(IConfiguration configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }
    }
}
