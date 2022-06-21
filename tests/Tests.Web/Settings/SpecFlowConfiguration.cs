using Microsoft.Extensions.Configuration;

namespace Tests.Web.Settings
{
    public class SpecFlowConfiguration : Tests.Abstractions.Settings.SpecFlowConfiguration
    {
        public SpecFlowConfiguration(IConfiguration configuration) : base(configuration)
        {

        }

        public string AccessibilityPrefix { get; set; }
        public string AccessibilityTag { get; set; }

        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string WebUrl { get; set; }
        public bool Screenshots { get; set; }
        
        public BrowserWindow Browser { get; set; }

        public class BrowserWindow
        {
            public int[] Size { get; set; }
            public bool Maximized { get; set; }
            public bool Hidden { get; set; }
        }
    }
}
