using System.Collections.Generic;

namespace Tests.Abstractions.Interfaces
{
    public interface IAutomationConfiguration
    {
        List<string> RequiredTags { get; }
        List<string> RequiredTagPrefixes { get; }
    }
}
