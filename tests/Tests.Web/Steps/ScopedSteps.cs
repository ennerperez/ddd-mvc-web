using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit.Framework;

namespace Tests.Web.Steps
{
    [Binding]
    public sealed class ScopedSteps
    {
        [Given("I have a valid configuration")]
        public Task ValidateConfigurationAsync()
        {
            return Task.CompletedTask;
        }

        [When("I initialize the application")]
        public Task InitializedApplicationAsync()
        {
            return Task.CompletedTask;
        }

        [Then("I should get a valid run")]
        public Task GetValidRunAsync()
        {
            return Task.CompletedTask;
        }

        [Given(@"The application is running")]
        public void GivenTheApplicationIsRunning()
        {
            try
            {
                var processs = new Process();
                var path = Path.Combine(Directory.GetCurrentDirectory(),"..","..","..","..","..");
                processs.StartInfo = new ProcessStartInfo("dotnet", "run") { WorkingDirectory = path };

                processs.Start();

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
