namespace Tests.Abstractions.Interfaces
{
    public interface IStepHelper
    {
        public IAutomationContext AutomationContext { get; }
        public IAutomationConfiguration AutomationConfigurations { get; }
    }
}
