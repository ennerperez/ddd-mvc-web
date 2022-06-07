using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Persistence.Contexts;
using Tests.Abstractions.Interfaces;

#if NUNIT
using NUnit.Framework;
#elif XUNIT
using Xunit;
#endif

// ReSharper disable InconsistentNaming

namespace Tests.Business.Steps
{
    [Binding]
    internal class ScopedSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly IAutomationConfiguration _automationConfiguration;
        private readonly IAutomationContext _automationContext;
        private readonly DefaultContext _defaultContext;

        private string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

        public ScopedSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext, DefaultContext defaultContext)
        {
            _automationConfiguration = automationConfiguration;
            _automationContext = automationContext;
            _defaultContext = defaultContext;
        }

        [Given("I have a valid configuration")]
        public Task ValidateConfigurationAsync()
        {
            return Task.CompletedTask;
        }

        [When("I initialize the application")]
        public Task InitializedApplicationAsync()
        {
            return Task.CompletedTask;
        }

        [Then("I should get a valid run")]
        public Task GetValidRunAsync()
        {
            return Task.CompletedTask;
        }
    }
}
