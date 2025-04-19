using System.Threading.Tasks;
using Tests.Abstractions.Interfaces;
#if USING_REQNROLL
using Reqnroll;

#else
using Test.Framework.Extended;
#endif

namespace Tests.IntegrationTests.Interfaces
{
    public interface ITestService
    {
        IAutomationContext AutomationContext { get; set; }
        Task CreateAsync(Table table);
        Task UpdateAsync(Table table);
        Task PartialUpdateAsync(Table table);
        Task DeleteAsync(Table table);
    }

    public interface ITestService<TEntity> : ITestService
    {
        Task<TEntity> ReadAsync(Table table);
    }
}
