using System;

namespace Infrastructure.Interfaces
{
    public interface ITenantService
    {
        string Tenant { get; }

        void SetTenant(string contextName, string tenant);

        string[] GetTenants(string contextName);

        event EventHandler OnTenantChanged;
    }
}
