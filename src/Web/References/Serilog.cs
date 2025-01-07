using System;
using Microsoft.AspNetCore.Http;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Enrichers.ClientInfo;
using Serilog.Events;

namespace Serilog
{
    public static class Extensions
    {
        public static LoggerConfiguration WithClientIp(this LoggerEnrichmentConfiguration enrichmentConfiguration, string xForwardHeaderName = null)
        {
            if (enrichmentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(enrichmentConfiguration));
            }

            if (!string.IsNullOrEmpty(xForwardHeaderName))
            {
                ClientIpConfiguration.XForwardHeaderName = xForwardHeaderName;
            }

            return enrichmentConfiguration.With<ClientIpEnricher>();
        }

        public static LoggerConfiguration WithClientAgent(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            if (enrichmentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(enrichmentConfiguration));
            }

            return enrichmentConfiguration.With<ClientAgentEnricher>();
        }
    }

    namespace Enrichers.ClientInfo
    {
        internal class ClientIpConfiguration
        {
            public static string XForwardHeaderName { get; set; } = "X-forwarded-for";
        }

        public class ClientAgentEnricher : ILogEventEnricher
        {
            private const string ClientAgentPropertyName = "ClientAgent";
            private const string ClientAgentItemKey = "Serilog_ClientAgent";

            private readonly IHttpContextAccessor _contextAccessor;

            public ClientAgentEnricher() : this(new HttpContextAccessor())
            {
            }

            internal ClientAgentEnricher(IHttpContextAccessor contextAccessor)
            {
                _contextAccessor = contextAccessor;
            }

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                var httpContext = _contextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return;
                }

                if (httpContext.Items[ClientAgentItemKey] is LogEventProperty logEventProperty)
                {
                    logEvent.AddPropertyIfAbsent(logEventProperty);
                    return;
                }

                var agentName = httpContext.Request.Headers["User-Agent"];

                var clientAgentProperty = new LogEventProperty(ClientAgentPropertyName, new ScalarValue(agentName));
                httpContext.Items.Add(ClientAgentItemKey, clientAgentProperty);

                logEvent.AddPropertyIfAbsent(clientAgentProperty);
            }
        }

        public class ClientIpEnricher : ILogEventEnricher
        {
            private const string IpAddressPropertyName = "ClientIp";
            private const string IpAddressForwaredForPropertyName = "ForwardedFor";
            private const string IpAddressItemKey = "Serilog_ClientIp";
            private const string IpAddressForwaredForItemKey = "Serilog_ForwaredFor";

            private readonly IHttpContextAccessor _contextAccessor;

            public ClientIpEnricher() : this(new HttpContextAccessor())
            {
            }

            internal ClientIpEnricher(IHttpContextAccessor contextAccessor)
            {
                _contextAccessor = contextAccessor;
            }

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                var httpContext = _contextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return;
                }

                if (httpContext.Items[IpAddressItemKey] is LogEventProperty logEventProperty)
                {
                    logEvent.AddPropertyIfAbsent(logEventProperty);
                    return;
                }

                var ipAddress = GetRemoteIpAddress();
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    ipAddress = "unknown";
                }

                var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));
                httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);
                logEvent.AddPropertyIfAbsent(ipAddressProperty);

                var ipAddressForwaredFor = GetForwardedFor();
                if (string.IsNullOrWhiteSpace(ipAddressForwaredFor))
                {
                    ipAddressForwaredFor = "unknown";
                }

                var ipAddressForwaredForProperty = new LogEventProperty(IpAddressForwaredForPropertyName, new ScalarValue(ipAddressForwaredFor));
                httpContext.Items.Add(IpAddressForwaredForItemKey, ipAddressForwaredForProperty);
                logEvent.AddPropertyIfAbsent(ipAddressForwaredForProperty);
            }

            private string GetRemoteIpAddress() => _contextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            private string GetForwardedFor()
            {
                if (!string.IsNullOrEmpty(_contextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"]))
                {
                    return _contextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"];
                }

                return null;
            }
        }
    }
}
