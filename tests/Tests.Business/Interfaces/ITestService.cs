using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Tests.Business.Interfaces
{
	public interface ITestService
	{
		Task CreateAsync(Table table);
		Task UpdateAsync(Table table);
		Task DeleteAsync(Table table);
	}
}
