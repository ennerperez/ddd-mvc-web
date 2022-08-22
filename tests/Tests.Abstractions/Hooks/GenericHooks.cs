using System.Threading;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace Tests.Abstractions.Hooks
{
    [Binding]
    public sealed class GenericHooks
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks
        private static IConfiguration _configuration;

        public GenericHooks(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BeforeTestRun]
        public static void BeforeTestRunInjection(ITestRunnerManager testRunnerManager, ITestRunner testRunner)
        {
        }

        [AfterTestRun]
        public static void AfterTestRunInjection(ITestRunnerManager testRunnerManager, ITestRunner testRunner)
        {
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:BeforeScenario") ?? 0);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:AfterScenario") ?? 0);
        }

        [BeforeFeature]
        public static void BeforeFeature()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:BeforeFeature") ?? 0);
        }

        [AfterFeature]
        public static void AfterFeature()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:AfterFeature") ?? 0);
        }

        [BeforeStep]
        public static void BeforeStep()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:BeforeStep") ?? 0);
        }

        [AfterStep]
        public static void AfterStep()
        {
            Thread.Sleep(_configuration?.GetValue<int>("Timeouts:AfterStep") ?? 0);
        }

    }
}
