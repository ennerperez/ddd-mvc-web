using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Tests.Web.Steps
{
    public partial class ScopedSteps
    {
        [Given("Tengo una configuraci[o|ó]n v[a|á]lida")]
        public Task GivenValidateConfigurationAsync()
        {
            return ValidateConfigurationAsync(nameof(GivenValidateConfigurationAsync));
        }

        [When("Inicializo la aplicaci[o|ó]n")]
        public Task WhenInitializedApplicationAsync()
        {
            return InitializedApplicationAsync(nameof(WhenInitializedApplicationAsync));
        }

        [Then("Tengo una ejecuci[o|ó]n v[a|á]lida")]
        public Task ThenGetValidRunAsync()
        {
            return GetValidRunAsync(nameof(ThenGetValidRunAsync));
        }
    }
}
