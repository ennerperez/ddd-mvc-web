using System.Threading;

namespace Infrastructure.Interfaces
{
    public interface IVaultService<T> where T : class
    {
        T GetSecret(string key, string version, CancellationToken cancellationToken);
        T SetSecret(string key, string value, CancellationToken cancellationToken);
    }
}
