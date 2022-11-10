using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Test.Framework.Extended;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Business.Interfaces;
using Xunit.Sdk;

// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming

namespace Tests.Business.Steps
{
	[Binding]
	public class ScopedSteps
	{
		// For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

		private readonly IAutomationConfiguration _automationConfiguration;
		private readonly IAutomationContext _automationContext;
		protected readonly LoremIpsumService _loremIpsumService;

		// ReSharper disable once UnusedMember.Local
		private string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

		public ScopedSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext, LoremIpsumService loremIpsumService)
		{
			_automationConfiguration = automationConfiguration;
			_automationContext = automationContext;
			_loremIpsumService = loremIpsumService;
		}

		[Given(@"(?:a|an) (.*) (?:using|with) the following (?:data|information)")]
		[Given(@"the following (.*) (data|information)")]
		public Task GivenSetEntityAsync(string type, Table table)
		{
			foreach (var row in table.Rows)
			{
				row["Field"] = row["Field"].Replace(" ", string.Empty);
				row["Value"] = row["Value"].EvaluateString();
			}
			_automationContext.SetAttributeInAttributeLibrary("this", type);
			_automationContext.SetAttributeInAttributeLibrary($"{_scenarioCode}_{type}".ToLower(), table);

			return Task.CompletedTask;
		}

		[Then(@"the (.*) (?:must|should|will) be successfully (created|updated|deleted)")]
		[Then(@"(this|it) (?:must|should|will) be successfully (created|updated|deleted)")]
		public async Task ThenOperationResultAsync(string type, string operation)
		{
			if (type == "this")
				type = _automationContext.GetAttributeFromAttributeLibrary(type, false).ToString();
			var table = (Table)_automationContext.GetAttributeFromAttributeLibrary($"{_scenarioCode}_{type}".ToLower());
			await GivenManipulateEntityAsync(type, operation, table);
		}

		[Given(@"the (.*) (?:must|should|will) be (created|updated|deleted) (?:using|with) the following (?:data|information)")]
		public async Task GivenManipulateEntityAsync(string type, string operation, Table table)
		{
			_automationContext.SetAttributeInAttributeLibrary($"{_scenarioCode}_{type}".ToLower(), table);
			try
			{
				var service = Program.Container.GetServices(typeof(ITestService))
					.FirstOrDefault(s=> s != null && s.GetType().Name.Equals($"{type}TestService", StringComparison.InvariantCultureIgnoreCase));
				if (service == null) throw new NullException("Unable to find an instance for the required service");

				var methodName = operation switch
				{
					"created" => $"CreateAsync",
					"updated" => $"UpdateAsync",
					"deleted" => $"DeleteAsync",
					_ => throw new ArgumentException("Method is not allowed")
				};

				var method = service.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				dynamic awaitable = method?.Invoke(service, new object[] {table});
				if (awaitable == null) throw new NullException(methodName);
				await awaitable;
				Assert.Pass();
			}
			catch (NotImplementedException)
			{
				throw;
			}
			catch (Exception e)
			{
				Assert.Fail(e);
			}
		}

	}
}
