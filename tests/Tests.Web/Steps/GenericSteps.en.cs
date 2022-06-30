using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Steps
{
    internal partial class GenericSteps
    {
        [When(@"I click on the ""(.*)"" (button|tab|link|element)")]
        public void WhenIClickOn(string name, string control)
        {
            IClickOn(name, control, nameof(WhenIClickOn));
        }
        
        [Then(@"I click on ""(.*)"" (button|tab|link|element)")]
        public void ThenIClickOn(string name, string control)
        {
            IClickOn(name, control, nameof(ThenIClickOn));
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
            IWriteOnInput(opcion, control, nameof(ThenIWriteOn));
        }
    }
}
