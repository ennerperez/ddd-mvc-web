using System.Threading.Tasks;
using Domain.Entities;

namespace Persistence.Interfaces
{
    public interface ISettingRepository : IGenericRepository<Setting>
    {
        string GetValue(string key);
        TValue GetValue<TValue>(string key) where TValue : struct;

        Task<TValue> GetValueAsync<TValue>(string key) where TValue : struct;

        Task<string> GetValueAsync(string key);
    }
}
