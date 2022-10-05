using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
// ReSharper disable NotAccessedField.Local

// ReSharper disable UnusedParameter.Local

namespace Tests.Web.Steps
{
	[Binding]
	public partial class ScopedSteps
	{

		// For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

		private readonly IAutomationConfiguration _automationConfiguration;
		private readonly IAutomationContext _automationContext;

		// ReSharper disable once UnusedMember.Local
		private string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

		public ScopedSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext)
		{
			_automationConfiguration = automationConfiguration;
			_automationContext = automationContext;
		}

		private Task ValidateConfigurationAsync(string method)
		{
			return Task.CompletedTask;
		}

		private Task InitializedApplicationAsync(string method)
		{
			return Task.CompletedTask;
		}

		private Task GetValidRunAsync(string method)
		{
			return Task.CompletedTask;
		}

	}
}
