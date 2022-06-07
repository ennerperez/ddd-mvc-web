using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Settings
{
    public class SpecFlowConfiguration : IAutomationConfiguration
    {
        public List<string> RequiredTags { get; }
        public List<string> RequiredTagPrefixes { get; }

        public SpecFlowConfiguration(IConfiguration configuration)
        {
            RequiredTags = new List<string>();
            RequiredTagPrefixes = new List<string>();

            configuration.Bind("requiredTags", RequiredTags);
            configuration.Bind("requiredTagPrefixes", RequiredTagPrefixes);
        }
    }
}
