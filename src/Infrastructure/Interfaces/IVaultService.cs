using System.Threading;

namespace Infrastructure.Interfaces
{
    public interface IVaultService<out T> where T : class
    {
        T GetSecret(string key, string version, CancellationToken cancellationToken);
        T SetSecret(string key, string value, CancellationToken cancellationToken);
    }
}
