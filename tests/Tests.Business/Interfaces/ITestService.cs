using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

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
