using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailAsync(string email, string userFullName, string subject, string htmlMessage, bool useThread = false, string bcc = "", Dictionary<string, byte[]> attachments = null);
    }
}
