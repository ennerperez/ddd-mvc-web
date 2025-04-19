using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Reqnroll;
using Shouldly;
using Tests.Abstractions.Interfaces;
using Xunit;

namespace Tests.UnitTests.Steps
{
    [Binding]
    public class ScopedSteps
    {
        private readonly IAutomationContext _automationContext;

        public ScopedSteps(IAutomationContext automationContext)
        {
            _automationContext = automationContext;
        }

        private string ScenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

        private class TestDocument : IDocument
        {
            public TestDocument(object model)
            {
                Title = model.ToString();
            }

            public string Title { get; set; }
            public string FileName { get; set; }
        }

        [Given("the (.*) compose a new (.*) document")]
        public async Task GivenComposeANewDocument(string subject, string title)
        {
            var service = _automationContext.Container.GetService<IDocumentService>();
            var result = service.Compose<TestDocument>(new TestDocument(title));
            var resultAsync = await service.ComposeAsync<TestDocument>(new TestDocument(title));
            result.ShouldNotBeNull();
            resultAsync.ShouldNotBeNull();
            _automationContext.SetAttribute(title, result);
        }

        [When(@"the (.*) document is generated as (.*)")]
        [Then(@"the (.*) document is generated as (.*)")]
        public async Task WhenDocumentIsGeneratedAs(string title, string format)
        {
            var document = (TestDocument)_automationContext.GetAttribute(title);
            var service = _automationContext.Container.GetService<IDocumentService>();
            var result = service.Generate(document, format);
            var resultAsync = await service.GenerateAsync(document, format);
            result.ShouldNotBeNull();
            resultAsync.ShouldNotBeNull();
        }

        [Given(@"the (.*) create a new (.*) file")]
        public async Task GivenTheUserCreateANewFile(string subject, string fileName)
        {
            var service = _automationContext.Container.GetService<IFileService>();
            var tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            service.WriteAllText(tempFileName, DateTime.Now.Ticks.ToString());
            await service.WriteAllTextAsync(tempFileName, DateTime.Now.Ticks.ToString());
            _automationContext.SetAttribute("FileName", tempFileName);
        }

        [Then(@"the (.*) file should be created")]
        public async Task ThenTheFileTestShouldBeCreated(string fileName)
        {
            var service = _automationContext.Container.GetService<IFileService>();
            var tempFileName = Path.Combine(Path.GetTempPath(), fileName);
            service.Exists(tempFileName).ShouldBeTrue();
            (await service.ExistsAsync(tempFileName)).ShouldBeTrue();
        }

        [Given(@"the (.*) send an email to (.*)")]
        public async Task GivenTheServiceSendAnEmailTo(string subject, string email)
        {
            //var service = _automationContext.Container.GetService<IEmailService>();
            var service = Substitute.For<IEmailService>();
            await service.SendEmailAsync(email, subject, string.Empty);
            Assert.True(true);
            _automationContext.SetAttribute("email_to", email);
        }

        [Then("the email should be received")]
        public void ThenTheEmailShouldBeReceived()
        {
            var email = _automationContext.GetAttribute("email_to")?.ToString();
            email.ShouldNotBeNull();
        }

        [Given("the following data")]
        public void GivenTheFollowingData(Table table)
        {
            _automationContext.SetAttribute("entity_fields", table);
            table.ShouldNotBeNull();
        }

        [Then(@"(?:a|an) (.*) should be created")]
        public void ThenAEntityShouldBeCreated(string typeName)
        {
            var table = _automationContext.GetAttribute<Table>("entity_fields");
            var @namespace = _automationContext.GetAttribute<string>("namespace");
            var assemblyName = _automationContext.GetAttribute<string>("assembly");
            var type = Type.GetType($"{@namespace}.{typeName}, {assemblyName}");
            type.ShouldNotBeNull();

            var instance = Activator.CreateInstance(type);
            instance.ShouldNotBeNull();

            foreach (var field in table.Rows)
            {
                var ptype = type.GetProperty(field["Field"])?.PropertyType;
                var pvalue = field["Value"].Evaluate(ptype);
                type.GetProperty(field["Field"])?.SetValue(instance, pvalue);
            }

            _automationContext.RemoveAttribute("entity_fields");

            var fields = table.Rows.Select(m => m["Field"]).ToArray();
            foreach (var prop in type.GetProperties().Where(m => fields.Contains(m.Name)))
            {
                prop.GetValue(instance).ShouldNotBeNull(prop.Name);
            }
        }

        [Given("the (.*) namespace in the (.*) assembly")]
        public void GivenTheNamespaceInAssembly(string @namespace, string assembly)
        {
            @namespace.ShouldNotBeNull();
            assembly.ShouldNotBeNull();
            _automationContext.SetAttribute("namespace", @namespace);
            _automationContext.SetAttribute("assembly", assembly);
        }

        [Then(@"all (.*) ctors should be used")]
        public void ThenAllCtorsShouldBeUsed(string typeName)
        {
            var @namespace = _automationContext.GetAttribute<string>("namespace");
            var assemblyName = _automationContext.GetAttribute<string>("assembly");
            var type = Type.GetType($"{@namespace}.{typeName}, {assemblyName}");
            type.ShouldNotBeNull();

            var ctors = type.GetConstructors();
            ctors.ShouldNotBeNull();

            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == 0)
                {
                    continue;
                }

                //var values = parameters.Select(m => m.ParameterType.GetMethod("GetDefaultGeneric")?.MakeGenericMethod(m.ParameterType).Invoke(m.ParameterType, null)).ToArray();
                var values = parameters.Select(m => m.ParameterType.GetDefaultValue()).ToArray();
                Activator.CreateInstance(type, values).ShouldNotBeNull();
            }
        }
    }
}
