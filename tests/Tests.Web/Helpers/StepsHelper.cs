using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Helpers
{
    public class StepsHelper : Abstractions.Helpers.StepsHelper
    {

        private FeatureContext _featureContext => _automationContext.FeatureContext;
        private ScenarioContext _scenarioContext => _automationContext.ScenarioContext;

        public StepsHelper(IAutomationContext automationContext, IAutomationConfiguration automationConfigurations) : base(automationContext, automationConfigurations)
        {
        }

        private static int _counter = 1;

        private string GetScreenshotFileName(string method = "")
        {
            //if (string.IsNullOrWhiteSpace(method)) method = scenarioContext.StepContext.StepInfo.Text;
            var featureTitle = (_featureContext != null && _featureContext.FeatureInfo != null ? _featureContext.FeatureInfo.Title : string.Empty);
            var scenarioTitle = (_scenarioContext != null && _scenarioContext.ScenarioInfo != null ? _scenarioContext.ScenarioInfo.Title : string.Empty);

            var l = 3;
            if (_counter.ToString().Length > l)
            {
                l = _counter.ToString().Length;
            }

            var keys = new[] { DateTime.Now.ToString("yyyyMMddHHmmss"), featureTitle, scenarioTitle, _counter.ToString($"D{l}"), method }.Where(m => !string.IsNullOrWhiteSpace(m));
            var fileName = $"{string.Join("_", keys)}.jpg";
            fileName = fileName.Replace(" ", "_").Replace("-", "").Replace("__", "_");
            var name = Path.Combine("screenshots", fileName);
            _counter++;
            return name;
        }

        public override void CaptureTakeScreenshot(object driver, string method = "", bool trace = false)
        {
            try
            {
                Thread.Sleep(Program.Configuration?.GetValue<int>("Timeouts:BeforeScreenshot") ?? 0);
                var takesScreenshot = driver as ITakesScreenshot;
                var fileName = GetScreenshotFileName(method);
                var screenshot = takesScreenshot?.GetScreenshot();
                var directory = Path.GetDirectoryName(fileName) ?? "";
                var name = Path.GetFileName(fileName);
                fileName = Path.Combine(directory, name);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (screenshot != null)
                {
                    string imageBase64;
                    try
                    {
                        var buffer = screenshot.AsByteArray;
                        using var image = Image.Load(buffer);
                        var width = 192;
                        image.Mutate(x => x.Resize(width, 0));
                        imageBase64 = image.ToBase64String(SixLabors.ImageSharp.Formats.Jpeg.JpegFormat.Instance);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        imageBase64 = screenshot.AsBase64EncodedString;
                    }

                    if (trace)
                    {
                        Console.WriteLine($@"SCREENSHOT[ {imageBase64} ]SCREENSHOT");
                    }
                    //Console.WriteLine($"SCREENSHOT_PATH[ {path} ]SCREENSHOT_PATH");
                    screenshot.SaveAsFile(fileName);
                }

                Thread.Sleep(Program.Configuration?.GetValue<int>("Timeouts:AfterScreenshot") ?? 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error while taking screenshot: {0}", ex);
            }
        }
    }
}
