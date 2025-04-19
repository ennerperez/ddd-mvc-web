using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Reqnroll;
using SkiaSharp;
using Tests.Abstractions.Interfaces;

namespace Tests.UITests.Helpers
{
    public class StepsHelper : Abstractions.Helpers.StepsHelper
    {
        private static int s_counter = 1;

        public StepsHelper(IAutomationContext automationContext, IAutomationConfiguration automationConfigurations) : base(automationContext, automationConfigurations)
        {
        }

        private FeatureContext FeatureContext => AutomationContext.FeatureContext;
        private ScenarioContext ScenarioContext => AutomationContext.ScenarioContext;

        private string GetScreenshotFileName(string method = "")
        {
            //if (string.IsNullOrWhiteSpace(method)) method = scenarioContext.StepContext.StepInfo.Text;
            var featureTitle = this.FeatureContext != null && this.FeatureContext.FeatureInfo != null ? this.FeatureContext.FeatureInfo.Title : string.Empty;
            var scenarioTitle = this.ScenarioContext != null && this.ScenarioContext.ScenarioInfo != null ? this.ScenarioContext.ScenarioInfo.Title : string.Empty;

            var l = 3;
            if (s_counter.ToString().Length > l)
            {
                l = s_counter.ToString().Length;
            }

            var keys = new[] { DateTime.Now.ToString("yyyyMMddHHmmss"), featureTitle, scenarioTitle, s_counter.ToString($"D{l}"), method }.Where(m => !string.IsNullOrWhiteSpace(m));
            var fileName = $"{string.Join("_", keys)}.jpg";
            fileName = fileName.Replace(" ", "_").Replace("-", "").Replace("__", "_");
            var name = Path.Combine("screenshots", fileName);
            s_counter++;
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
                        using var skImage = SKBitmap.Decode(screenshot.AsByteArray);
                        int newHeight = (int)(skImage.Height * 0.5);
                        int newWidth = (int)(skImage.Width * 0.5);
                        using var scaledBitmap = skImage.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
                        using var image = SKImage.FromBitmap(scaledBitmap);
                        using var encodedImage = image.Encode(SKEncodedImageFormat.Jpeg, 80);
                        imageBase64 = Convert.ToBase64String(encodedImage.ToArray());
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
