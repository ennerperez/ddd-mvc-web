namespace Tests.Abstractions.Interfaces
{
	public interface IDefinitionConfiguration
	{
		string ElementUniquenessIdentifier { get; }
		string MustAwaitElementLoadIdentifier { get; }
		string OptionalElementIdentifier { get; }

		string GherkinInlineNewLine { get; }
		string GherkinTableWhitespace { get; }
		int ImplicitElementWaitTime { get; }
		int MaxSlowLoadingElementWaitTime { get; }
		int DefaultScenarioRunSlowdownTime { get; }
		int ElementSearchRetryFactor { get; }
		string GetApplicationDefinitionsLocation();

	}
}
