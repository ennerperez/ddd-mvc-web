using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Steps
{
    public abstract class GenericSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly IAutomationConfiguration _automationConfiguration;
        private readonly IAutomationContext _automationContext;
        private readonly IStepHelper _stepsHelper;

        public GenericSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext, IStepHelper stepsHelper)
        {
            _automationConfiguration = automationConfiguration;
            _automationContext = automationContext;
            _stepsHelper = stepsHelper;
        }
    }
}
