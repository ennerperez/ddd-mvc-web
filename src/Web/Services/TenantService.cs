#if USING_MULTITENANCY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Web.Services
{
    public partial class TenantService : ITenantService
    {
        [GeneratedRegex(@"(.*)\[(.*)\]\.(\w+)", RegexOptions.Compiled)]
        private static partial Regex MultiTenantCsRegex();

        private readonly Dictionary<string, string[]> _tenants;

        public TenantService(IConfiguration configuration)
        {
            var connectionStrings = new Dictionary<string, string>();
            configuration.Bind(key: "ConnectionStrings", connectionStrings);

            _tenants = connectionStrings.Select(m => MultiTenantCsRegex().Match(m.Key))
                .Where(m => m.Success)
                .GroupBy(m => m.Groups[1].Value)
                .ToDictionary(k => k.Key, v => v.Select(m => m.Groups[2].Value).Distinct().ToArray());
        }

        public string Tenant { get; private set; }

        public void SetTenant(string contextName, string tenant)
        {
            if (!_tenants.ContainsKey(contextName) || !_tenants[contextName].Contains(tenant))
            {
                throw new ArgumentException($"Tenant '{tenant}' is not a valid tenant for {contextName}.");
            }

            Tenant = tenant;
            OnTenantChanged?.Invoke(this, EventArgs.Empty);
        }

        public string[] GetTenants(string contextName)
        {
            return !_tenants.TryGetValue(contextName, out string[] tenant) ? [] : tenant.ToArray();
        }

        public event EventHandler OnTenantChanged;
    }
}
#endif
