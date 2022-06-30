using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Persistence.Contexts;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Xunit.Framework;

namespace Tests.Web.Steps
{
    [Binding]
    public partial class ScopedSteps
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
