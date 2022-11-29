using System;
using System.IO;
using System.Reflection;
using Tests.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Tests.Web.Settings
{
  public class DefinitionConfiguration : IDefinitionConfiguration
  {
    private readonly IConfiguration _configuration;

    public DefinitionConfiguration(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    #region DefinitionSettings

    public string ElementUniquenessIdentifier => _configuration["DefinitionSettings:ElementUniquenessIdentifier"];
    public string MustAwaitElementLoadIdentifier => _configuration["DefinitionSettings:MustAwaitElementLoadIdentifier"];
    public string OptionalElementIdentifier => _configuration["DefinitionSettings:OptionalElementIdentifier"];
    public string GherkinInlineNewLine => _configuration["DefinitionSettings:GherkinInlineNewLine"];
    public string GherkinTableWhitespace => _configuration["DefinitionSettings:GherkinTableWhitespace"];
    public int ImplicitElementWaitTime => _configuration?.GetValue<int>("DefinitionSettings:ImplicitElementWaitTime") ?? 0;
    public int MaxSlowLoadingElementWaitTime => _configuration?.GetValue<int>("DefinitionSettings:MaxSlowLoadingElementWaitTime") ?? 0;
    public int DefaultScenarioRunSlowdownTime => _configuration?.GetValue<int>("DefinitionSettings:DefaultScenarioRunSlowdownTime") ?? 0;
    public int ElementSearchRetryFactor => _configuration?.GetValue<int>("DefinitionSettings:ElementSearchRetryFactor") ?? 0;

    #endregion

    public string GetApplicationDefinitionsLocation()
    {
      var path = _configuration["DefinitionSettings:Location"];
      if (string.IsNullOrWhiteSpace(path))
      {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        Path.Combine(assemblyName ?? throw new InvalidOperationException("AssemblyName"), "Definitions");
      }

      return path;
    }

  }
}
