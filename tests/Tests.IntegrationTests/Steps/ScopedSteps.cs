using System;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Test.Framework.Extended;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.IntegrationTests.Interfaces;
using Xunit.Sdk;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming

namespace Tests.IntegrationTests.Steps
{
    [Binding]
    public class ScopedSteps
    {

        private readonly IAutomationContext _automationContext;
        protected readonly LoremIpsumService _loremIpsumService;

        public ScopedSteps(IAutomationContext automationContext, LoremIpsumService loremIpsumService)
        {
            _automationContext = automationContext;
            _loremIpsumService = loremIpsumService;
        }

        private string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

        #region New Methods

        [Given(@"(?:a|an) (.*) (?:using|with) the following (?:data|information)")]
        [Given(@"the following (.*) (?:data|information)")]
        public Task GivenSetEntity(string type, Table table)
        {
            foreach (var row in table.Rows)
            {
                row["Field"] = row["Field"].Replace(" ", string.Empty);
                row["Value"] = row["Value"].Evaluate();
            }

            _automationContext.SetAttribute("this", type);
            _automationContext.SetAttribute($"{_scenarioCode}_{type}".ToLower(), table);
            Assert.Pass();
            return Task.CompletedTask;
        }

        [Then(@"the (.*) (?:must|should|will) (not be|be) successfully (created|updated|partially updated|deleted)")]
        public async Task ThenOperationResult(string type, string denied, string operation)
        {
            var isDenied = denied.Equals("not be");
            if (type == "this")
            {
                type = _automationContext.GetAttribute(type, false).ToString();
            }

            var result = _automationContext.GetAttribute($"{_scenarioCode}_{type}".ToLower(), false);
            if (result != null && result.GetType() == typeof(Table))
            {
                await GivenManipulateEntity(type, denied, operation, (Table)result);
            }
            else if (result != null && result.GetType().IsAssignableTo(typeof(IEnumerable)) && (result as IEnumerable ?? throw new InvalidOperationException()).ToDynamicArray().Length == 0)
            {
                if (isDenied)
                {
                    Assert.Pass();
                }
                else
                {
                    //Assert.Fail(new EmptyException((result as IEnumerable)));
                    Assert.Fail(NotEmptyException.ForNonEmptyCollection());
                }
            }
            else if (result != null)
            {
                if (isDenied)
                {
                    //Assert.Fail(new EmptyException(new[] { result }));
                    Assert.Fail(NotEmptyException.ForNonEmptyCollection());
                }
                else
                {
                    Assert.Pass();
                }
            }

            switch (isDenied)
            {
                case true when _automationContext.TestError != null:
                case false when _automationContext.TestError == null:
                    Assert.Pass();
                    break;
                case true when _automationContext.TestError == null:
                    Assert.Fail(new ArgumentException("A result was returned when none was expected"));
                    break;
                case false when _automationContext.TestError != null:
                    Assert.Fail(_automationContext.TestError);
                    break;
            }
        }

        [Given(@"the (.*) (?:must|should|will) (not be|be)? (created|updated|partially updated|deleted) (?:using|with) the following (?:data|information)")]
        public async Task GivenManipulateEntity(string type, string denied, string operation, Table table)
        {
            var isDenied = denied.Equals("not be");
            foreach (var row in table.Rows)
            {
                row["Field"] = row["Field"].Replace(" ", string.Empty);
                row["Value"] = row["Value"].Evaluate();
            }

            _automationContext.SetAttribute($"{_scenarioCode}_{type}".ToLower(), table);
            try
            {
                var serviceType = Assembly.GetExecutingAssembly().DefinedTypes.FirstOrDefault(m => m.Name.Equals($"{type}TestService", StringComparison.InvariantCultureIgnoreCase));
                var serviceInterface = serviceType?.ImplementedInterfaces.FirstOrDefault() ?? throw NotNullException.ForNullValue();
                var service = (ITestService)Program.Container.GetServices(serviceInterface)
                    .FirstOrDefault(s => s != null && s.GetType().Name.Equals($"{type}TestService", StringComparison.InvariantCultureIgnoreCase)) ?? throw NotNullException.ForNullValue();
                service.AutomationContext = _automationContext;

                var methodName = operation switch
                {
                    "created" => "CreateAsync",
                    "updated" => "UpdateAsync",
                    "partially updated" => "PartialUpdateAsync",
                    "deleted" => "DeleteAsync",
                    _ => throw new ArgumentException("Method is not allowed")
                };

                var method = service.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                dynamic awaitable = method?.Invoke(service, [table]) ?? throw NotNullException.ForNullValue();
                await awaitable;

                Assert.Pass();
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (isDenied)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail(e);
                }
            }
        }

        #endregion

    }
}
