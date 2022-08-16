using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Models;

namespace Web.Controllers.MVC
{
    [AllowAnonymous]
	[ApiExplorerSettings(IgnoreApi = true)]
    public class DefaultController : Controller
    {
        private readonly ILogger _logger;

        public DefaultController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IActionResult Index()
        {
            return View();
        }

		[Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("{TraceIdentifier}", HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("About")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About()
        {
            var domain = Assembly.GetAssembly(typeof(Domain.Extensions));
            var infrastructure = Assembly.GetAssembly(typeof(Infrastructure.Extensions));
            var persistence = Assembly.GetAssembly(typeof(Persistence.Extensions));
            var business = Assembly.GetAssembly(typeof(Business.Extensions));
            var web = Assembly.GetAssembly(typeof(Program));

            var models = new List<AboutViewModel>();

            if (domain != null)
                models.Add(new()
                {
                    Name = domain.GetName().Name,
                    Title = domain.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = domain.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? "Domain Layer",
                    Version = domain.GetName().Version,
                    Published = System.IO.File.GetCreationTime(domain.Location),
                    Color = "#f54437"
                });

            if (infrastructure != null)
                models.Add(new()
                {
                    Name = infrastructure.GetName().Name,
                    Title = infrastructure.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = infrastructure.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? "Infrastructure Layer",
                    Version = infrastructure.GetName().Version,
                    Published = System.IO.File.GetCreationTime(infrastructure.Location),
                    Color = "#ea1f64"
                });

            if (persistence != null)
                models.Add(new()
                {
                    Name = persistence.GetName().Name,
                    Title = persistence.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = persistence.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? "Persistence Layer",
                    Version = persistence.GetName().Version,
                    Published = System.IO.File.GetCreationTime(persistence.Location),
                    Color = "#9d28b1",
                });

            if (business != null)
                models.Add(new()
                {
                    Name = business.GetName().Name,
                    Title = business.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = business.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? "Business Layer",
                    Version = business.GetName().Version,
                    Published = System.IO.File.GetCreationTime(business.Location),
                    Color = "#683bb8",
                });

            if (web != null)
                models.Add(new()
                {
                    Name = web.GetName().Name ?? "Web",
                    Title = web.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = web.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description ?? "Web Layer",
                    Version = web.GetName().Version,
                    Published = System.IO.File.GetCreationTime(web.Location),
                    Color = "#4052b6",
                });

            var assemblies = this.GetType().Assembly.GetReferencedAssemblies();

            foreach (var item in assemblies)
            {
                var model = new AboutViewModel() { Name = item.Name, Version = item.Version, Dependency = true, };
                if (!models.Any(m => m.Name == model.Name))
                    models.Add(model);
            }

            return View(models);
        }
    }
}
