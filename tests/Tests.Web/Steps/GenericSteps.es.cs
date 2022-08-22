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
        [When(@"Hago click en el (boton|tab|vinculo|elemento) ""(.*)""")]
        public void CuandoHagoClickEn(string control, string name)
        {
            IClickOn(name,control, nameof(CuandoHagoClickEn));
        }

        [Then(@"Hago click en el ""(.*)"" (boton|tab|vinculo|elemento)")]
        public void EntoncesHagoClickEn(string name, string control)
        {
            IClickOn(name,control, nameof(EntoncesHagoClickEn));
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
            IWriteOnInput(opcion, control, nameof(EntoncesEscriboEn));
        }
    }
}
