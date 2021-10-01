using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Models;

namespace Web.MVC.Controllers
{
    [AllowAnonymous]
    public class DefaultController : Controller
    {
        private readonly ILogger _logger;

        public DefaultController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError(HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult About()
        {
            var domain = Assembly.GetAssembly(typeof(Domain.Extensions));
            var infrastructure = Assembly.GetAssembly(typeof(Infrastructure.Extensions));
            var persistence = Assembly.GetAssembly(typeof(Persistence.Extensions));
            var web = Assembly.GetAssembly(typeof(Program));

            var models = new List<AboutViewModel>
            {
                new()
                {
                    Name = domain.GetName().Name,
                    Title = domain.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = domain.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description,
                    Version = domain.GetName().Version,
                    Published = System.IO.File.GetCreationTime(domain.Location),
                    Color = "#ed1c2e"
                },
                new()
                {
                    Name = infrastructure.GetName().Name,
                    Title = infrastructure.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = infrastructure.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description,
                    Version = infrastructure.GetName().Version,
                    Published = System.IO.File.GetCreationTime(infrastructure.Location),
                    Color = "#007bc3"
                },
                new()
                {
                    Name = persistence.GetName().Name,
                    Title = persistence.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = persistence.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description,
                    Version = persistence.GetName().Version,
                    Published = System.IO.File.GetCreationTime(persistence.Location),
                    Color = "#ff8202",
                },
                new()
                {
                    Name = web.GetName().Name,
                    Title = web.GetCustomAttributes(true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title,
                    Description = web.GetCustomAttributes(true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description,
                    Version = web.GetName().Version,
                    Published = System.IO.File.GetCreationTime(web.Location),
                    Color = "#2d572c",
                }
            };

            var assemblies = this.GetType().Assembly.GetReferencedAssemblies();

            foreach (var item in assemblies)
            {
                var model = new AboutViewModel()
                {
                    Name = item.Name, 
                    Version = item.Version, 
                    Dependency = true,
                };
                if (!models.Any(m => m.Name == model.Name))
                    models.Add(model);
            }

            return View(models);
        }
    }
}
