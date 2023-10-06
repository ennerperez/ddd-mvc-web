using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
namespace App.Services
{
    public class MauiHttpContextAccessor : IHttpContextAccessor
    {

        private HttpContext _httpContext;

        public HttpContext HttpContext { get => getFake(); set => _httpContext = value; }

        private HttpContext getFake()
        {
            var setups = new[] { new ConfigureOptions<FormOptions>(_ => new FormOptions()) };
            var postConfig = new[] { new PostConfigureOptions<FormOptions>("fake", _ => new FormOptions()) };

            var optionManager = new OptionsManager<FormOptions>(new OptionsFactory<FormOptions>(setups, postConfig));
            var a = new HttpContextFactory(optionManager, this);

            var features = new FeatureCollection();

            _httpContext = a.Create(features);
            return _httpContext;
        }
    }
}
