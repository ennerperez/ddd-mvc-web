using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit.Framework;

namespace Tests.Web.Steps
{
    public partial class ScopedSteps
    {
        [Given("I have a valid configuration")]
        public Task GivenValidateConfigurationAsync()
        {
            return ValidateConfigurationAsync(nameof(GivenValidateConfigurationAsync));
        }

        [When("I initialize the application")]
        public Task WhenInitializedApplicationAsync()
        {
            return InitializedApplicationAsync(nameof(WhenInitializedApplicationAsync));
        }

        [Then("I should get a valid run")]
        public Task ThenGetValidRunAsync()
        {
            return GetValidRunAsync(nameof(ThenGetValidRunAsync));
        }
    }
}
