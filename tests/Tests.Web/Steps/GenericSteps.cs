using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Steps
{
    [Binding]
    internal partial class GenericSteps
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

        private IWebDriver Driver => Program.Driver;

        private Settings.SpecFlowConfiguration SpecFlowConfiguration => Program.SpecFlowConfiguration;

        private string ScreenshotFileName =>
            Path.Combine("screenshots", $"{DateTime.Now:yyyyMMddHHmmss}_{string.Join('_', _automationContext.ScenarioContext.ScenarioInfo.Tags)}.jpg");

        #region Privates

        private void IAmAtThePage(string name, string method)
        {
            Driver.WaitForElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{name}", 1000);
            if (SpecFlowConfiguration.Screenshots) Driver.SaveScreenshot(ScreenshotFileName);
        }

        private void IClickOn(string name, string control, string method)
        {
            var element = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{name}");
            element.Click();
            if (SpecFlowConfiguration.Screenshots) Driver.SaveScreenshot(ScreenshotFileName);
        }

        private void IFillInTheFollowingForm(Table table, string method, string rowFieldName = "field", string rowValueField = "value")
        {
            foreach (var row in table.Rows)
            {
                var field = row[rowFieldName];
                var value = row[rowValueField];
                var entry = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{field}");
                entry.SendKeys(value);
                Thread.Sleep(1000);
                if (SpecFlowConfiguration.Screenshots) Driver.SaveScreenshot(ScreenshotFileName);
            }
        }

        private void IWriteOnInput(string option, string control, string method)
        {
            var select = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{control}");
            var input = select.FindElement(By.TagName("input"));
            input.SendKeys(option);
            input.SendKeys(Keys.Enter);
        }

        #endregion

    }
}
