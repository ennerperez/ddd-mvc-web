using System.Threading.Tasks;
using Tests.Abstractions.Interfaces;
#if USING_SPECFLOW
using TechTalk.SpecFlow;

#else
using Test.Framework.Extended;
#endif

namespace Tests.Business.Interfaces
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
