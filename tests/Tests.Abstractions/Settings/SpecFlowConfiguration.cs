using System.Collections.Generic;
using Tests.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Tests.Abstractions.Settings
{
  public class SpecFlowConfiguration : IAutomationConfiguration
  {

    public SpecFlowConfiguration(IConfiguration configuration)
    {
      RequiredTags = new List<string>();
      RequiredTagPrefixes = new List<string>();

      configuration.Bind("requiredTags", RequiredTags);
      configuration.Bind("requiredTagPrefixes", RequiredTagPrefixes);
    }

    public List<string> RequiredTags { get; }
    public List<string> RequiredTagPrefixes { get; }

  }
}
