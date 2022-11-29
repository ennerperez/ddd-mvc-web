namespace Tests.Abstractions.Interfaces
{
	public interface IStepHelper
	{
		public IAutomationContext AutomationContext { get; }
		public IAutomationConfiguration AutomationConfigurations { get; }
		void CaptureTakeScreenshot(object driver, string method = "", bool trace = false);
	}
}
