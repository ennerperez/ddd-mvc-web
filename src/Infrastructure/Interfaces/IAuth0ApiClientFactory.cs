#if USING_AUTH0
using Auth0.ManagementApi;

namespace Infrastructure.Interfaces
{
    public interface IAuth0ApiClientFactory
    {
        IManagementApiClient CreateClient();
    }
}
#endif
