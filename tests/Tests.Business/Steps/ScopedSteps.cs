using System.Threading.Tasks;
using MediatR;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;

#if NUNIT
using NUnit.Framework;
#elif XUNIT
using Xunit;
#endif

// ReSharper disable InconsistentNaming

namespace Tests.Business.Steps
{
    [Binding]
    public partial class ScopedSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly IAutomationConfiguration _automationConfiguration;
        private readonly IAutomationContext _automationContext;
        private readonly ISender _mediator;

        private string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();
        
        protected readonly LoremIpsumService _loremIpsumService;

        public ScopedSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext, ISender mediator, LoremIpsumService loremIpsumService)
        {
            _automationConfiguration = automationConfiguration;
            _automationContext = automationContext;
            _mediator = mediator;
            _loremIpsumService = loremIpsumService;
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
