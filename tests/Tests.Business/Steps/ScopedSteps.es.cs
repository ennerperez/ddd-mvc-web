using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Tests.Business.Steps
{
    public partial class ScopedSteps
    {
        [Given("Tengo una configuraci[o|ó]n v[a|á]lida")]
        public Task CuandoTengoConfiguracionValidaAsync()
        {
            return ValidateConfigurationAsync(nameof(CuandoTengoConfiguracionValidaAsync));
        }

        [When("Inicializo la aplicaci[o|ó]n")]
        public Task CuandoInicializoApplicacionAsync()
        {
            return InitializedApplicationAsync(nameof(CuandoInicializoApplicacionAsync));
        }

        [Then("Obtengo una configuraci[o|ó]n v[a|á]lida")]
        public Task EntoncesObtengoConfigurationValidaAsync()
        {
            return GetValidRunAsync(nameof(EntoncesObtengoConfigurationValidaAsync));
        }
    }
}
