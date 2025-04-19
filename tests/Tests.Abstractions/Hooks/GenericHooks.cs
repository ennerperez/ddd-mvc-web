#if USING_REQNROLL
using System.Threading;
using Microsoft.Extensions.Configuration;
using Reqnroll;

namespace Tests.Abstractions.Hooks
{
    [Binding]
    public sealed class GenericHooks
    {
        private static IConfiguration s_configuration;

        public GenericHooks(IConfiguration configuration)
        {
            s_configuration = configuration;
        }

        [BeforeScenario]
        public static void BeforeScenario() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:BeforeScenario") ?? 0);

        [AfterScenario]
        public static void AfterScenario() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:AfterScenario") ?? 0);

        [BeforeFeature]
        public static void BeforeFeature() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:BeforeFeature") ?? 0);

        [AfterFeature]
        public static void AfterFeature() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:AfterFeature") ?? 0);

        [BeforeStep]
        public static void BeforeStep() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:BeforeStep") ?? 0);

        [AfterStep]
        public static void AfterStep() => Thread.Sleep(s_configuration?.GetValue<int>("Timeouts:AfterStep") ?? 0);

#pragma warning disable IDE0060 // Remove unused parameter
        [BeforeTestRun]
        public static void BeforeTestRunInjection(ITestRunnerManager testRunnerManager, ITestRunner testRunner)
        {
        }

        [AfterTestRun]
        public static void AfterTestRunInjection(ITestRunnerManager testRunnerManager, ITestRunner testRunner)
        {
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
#endif
