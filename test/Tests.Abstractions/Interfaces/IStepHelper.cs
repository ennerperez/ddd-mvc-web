namespace Tests.Abstractions.Interfaces
{
    public interface IStepHelper
    {
        public IAutomationContext AutomationContext { get; }
        public IAutomationConfiguration AutomationConfiguration { get; }
        void CaptureTakeScreenshot(object driver, string method = "", bool trace = false);
    }
}
