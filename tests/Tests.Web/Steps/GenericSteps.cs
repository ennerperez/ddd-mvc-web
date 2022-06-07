using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Steps
{
    [Binding]
    internal class GenericSteps
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

        private void IClickOnButton(string name, string method)
        {
            var button = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{name}");
            button.Click();
            if (SpecFlowConfiguration.Screenshots) Driver.SaveScreenshot(ScreenshotFileName);
        }

        private void IClickOnTab(string name, string method)
        {
            var button = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{name}");
            button.Click();
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

        private void IWriteOn(string option, string control, string method)
        {
            var select = Driver.FindElementByAccessibilityId($"{SpecFlowConfiguration.AccessibilityPrefix}{control}");
            var input = select.FindElement(By.TagName("input"));
            input.SendKeys(option);
            input.SendKeys(Keys.Enter);
        }

        #endregion

        #region Steps (EN)

        [When(@"I click on ""(.*)"" button")]
        public void WhenIClickOnButton(string name)
        {
            IClickOnButton(name, nameof(WhenIClickOnButton));
        }

        [Then(@"I click on ""(.*)"" button")]
        public void ThenIClickOnButton(string name)
        {
            IClickOnButton(name, nameof(ThenIClickOnButton));
        }

        [Then(@"I click on ""(.*)"" tab")]
        public void ThenIClickOnTab(string name)
        {
            IClickOnTab(name, nameof(ThenIClickOnTab));
        }

        [When("I fill in the following form")]
        public void WhenIFillInTheFollowingForm(Table table)
        {
            IFillInTheFollowingForm(table, nameof(WhenIFillInTheFollowingForm));
        }

        [Then("I fill in the following form")]
        public void ThenIFillInTheFollowingForm(Table table)
        {
            IFillInTheFollowingForm(table, nameof(ThenIFillInTheFollowingForm));
        }

        [Given(@"I am at the ""(.*)"" page")]
        public void GivenIAmAtThePage(string name)
        {
            IAmAtThePage(name, nameof(GivenIAmAtThePage));
        }

        [Then(@"I should be at the ""(.*)"" page")]
        public void ThenIShouldBeAtThePage(string name)
        {
            IAmAtThePage(name, nameof(ThenIShouldBeAtThePage));
        }

        [Then(@"I write ""(.*)"" on ""(.*)""")]
        public void ThenIWriteOn(string opcion, string control)
        {
            IWriteOn(opcion, control, nameof(ThenIWriteOn));
        }

        #endregion

        #region Steps (ES)

        [When(@"Hago click en el boton ""(.*)""")]
        public void CuandoHagoClickEnElBoton(string name)
        {
            IClickOnButton(name, nameof(CuandoHagoClickEnElBoton));
        }

        [Then(@"Hago click en el boton ""(.*)""")]
        public void EntoncesHagoClickEnElBoton(string name)
        {
            IClickOnButton(name, nameof(EntoncesHagoClickEnElBoton));
        }

        [Then(@"Yo hago click en una pestaña ""(.*)""")]
        public void EntoncesYoHagoClickEnUnaPestaña(string name)
        {
            IClickOnTab(name, nameof(EntoncesYoHagoClickEnUnaPestaña));
        }

        [When("Completo el siguiente formulario")]
        public void CuenadoCompletoElSiguienteFormulario(Table table)
        {
            IFillInTheFollowingForm(table, nameof(CuenadoCompletoElSiguienteFormulario), "campo", "valor");
        }

        [Then("Completo el siguiente formulario")]
        public void EntoncesCompletoElSiguienteFormulario(Table table)
        {
            IFillInTheFollowingForm(table, nameof(EntoncesCompletoElSiguienteFormulario), "campo", "valor");
        }

        [Given(@"Estoy en la pagina ""(.*)""")]
        public void DadoEstoyEnLaPagina(string name)
        {
            IAmAtThePage(name, nameof(DadoEstoyEnLaPagina));
        }

        [Then(@"Debería estar en la página ""(.*)""")]
        public void EntoncesDeberíaEstarEnLaPagina(string name)
        {
            IAmAtThePage(name, nameof(EntoncesDeberíaEstarEnLaPagina));
        }

        [Then(@"Escribo ""(.*)"" en ""(.*)""")]
        public void EntoncesEscriboEn(string opcion, string control)
        {
            IWriteOn(opcion, control, nameof(EntoncesEscriboEn));
        }

        #endregion

        #region Customs (EN)

        #endregion
    }
}
