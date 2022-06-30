using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
     public class SmtpService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SmtpService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailAsync(email, null, subject, htmlMessage);
        }

        public async Task SendEmailAsync(string email, string userFullName, string subject, string htmlMessage, bool useThread = false, string bcc = "", Dictionary<string, byte[]> attachments = null)
        {

            var smtpSettings = _configuration.GetSection("SmtpSettings");
            
            var name = smtpSettings["Name"];
            var @from = smtpSettings["From"];
            var server = smtpSettings["Server"];
            var port = int.Parse(smtpSettings["Port"]);
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"]);
            var useDefaultCredentials = bool.Parse(smtpSettings["UseDefaultCredentials"]);
        
            var client = new SmtpClient(server, port) { EnableSsl = enableSsl, UseDefaultCredentials = useDefaultCredentials };
            if (!client.UseDefaultCredentials)
                client.Credentials = new NetworkCredential(username, password);

            var sender = new MailAddress(@from, name, System.Text.Encoding.UTF8);

            var target = new MailAddress(email, userFullName, System.Text.Encoding.UTF8);
            var html = AlternateView.CreateAlternateViewFromString(htmlMessage, null, MediaTypeNames.Text.Html);

            var message = new MailMessage(sender, target) { IsBodyHtml = true, Subject = subject };

            message.AlternateViews.Add(html);
            if (attachments != null)
                foreach (var item in attachments)
                    if (item.Value != null)
                        message.Attachments.Add(new Attachment(new MemoryStream(item.Value), item.Key));
#if !DEBUG
            if (!string.IsNullOrWhiteSpace(bcc))
                message.Bcc.Add(new MailAddress(bcc));
#endif
            if (useThread)
            {
                var thread = new Task(async () =>
                {
                    try
                    {
                        await client.SendMailAsync(message);
                    }
                    catch (Exception e)
                    {
                        if (_logger != null)
                            _logger.LogError(e,"{Message}", e.Message);
                    }
                });
                thread.Start();
            }
            else
            {
                try
                {
                    await client.SendMailAsync(message);
                }
                catch (Exception e)
                {
                    if (_logger != null)
                        _logger.LogError(e,"{Message}", e.Message);
                }
            }
        }
    }
}
