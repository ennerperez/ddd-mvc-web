using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Entities
{
	public sealed class Definition : IDisposable
	{

		public Definition(string name, string type) : this()
		{
			Name = name;
			Type = type;
		}
		public Definition()
		{
			Ids = new Dictionary<string, string>();
			AutomationIds = new Dictionary<string, string>();
			AutomationNames = new Dictionary<string, string>();
			AutomationCSSes = new Dictionary<string, string>();
			AutomationXPaths = new Dictionary<string, string>();
			AutomationClassNames = new Dictionary<string, string>();
			AutomationLinkTexts = new Dictionary<string, string>();
			AutomationPartialLinkTexts = new Dictionary<string, string>();

			MustAwaitElements = new Dictionary<string, string>();
			UniqueElements = new Dictionary<string, string>();
			OptionalElements = new Dictionary<string, string>();
		}

		public Dictionary<string, string> MustAwaitElements { get; private set; }
		public Dictionary<string, string> UniqueElements { get; private set; }
		public Dictionary<string, string> OptionalElements { get; private set; }

		public string Name { get; private set; }

		public string Type { get; private set; }

		public Dictionary<string, string> Ids { get; set; }
		public Dictionary<string, string> AutomationIds { get; set; }
		public Dictionary<string, string> AutomationNames { get; set; }
		public Dictionary<string, string> AutomationCSSes { get; set; }
		public Dictionary<string, string> AutomationXPaths { get; set; }
		public Dictionary<string, string> AutomationClassNames { get; set; }
		public Dictionary<string, string> AutomationLinkTexts { get; set; }
		public Dictionary<string, string> AutomationPartialLinkTexts { get; set; }

		public void PrepareAndValidate(IDefinitionConfiguration configuration)
		{
			var sources = new[] {Ids, AutomationIds, AutomationNames, AutomationCSSes, AutomationXPaths, AutomationClassNames, AutomationLinkTexts, AutomationPartialLinkTexts};
			var regex1 = new Regex(@$"(\{configuration.ElementUniquenessIdentifier})?(\{configuration.MustAwaitElementLoadIdentifier})?(?:\{configuration.ElementUniquenessIdentifier}|\{configuration.MustAwaitElementLoadIdentifier})?(.*)", RegexOptions.Compiled);

			var elements = sources.SelectMany(m => m)
				.Where(m => m.Key.StartsWith(configuration.ElementUniquenessIdentifier) || m.Key.StartsWith(configuration.MustAwaitElementLoadIdentifier))
				.ToArray();

			foreach (var key in elements)
			{
				var matches = regex1.Match(key.Key).Groups;
				var counter = matches.Count;
				if (counter >= 4)
				{
					var newKey = matches[3].Value;
					var source = sources.FirstOrDefault(m => m.ContainsKey(key.Key));
					source?.Remove(key.Key);
					source?.Add(newKey, key.Value);

					if (matches[1].Value == configuration.ElementUniquenessIdentifier || matches[2].Value == configuration.ElementUniquenessIdentifier)
						UniqueElements.Add(newKey, key.Value);
					if (matches[1].Value == configuration.MustAwaitElementLoadIdentifier || matches[2].Value == configuration.MustAwaitElementLoadIdentifier)
						MustAwaitElements.Add(newKey, key.Value);
				}
			}

			var regex2 = new Regex(@$"(\{configuration.OptionalElementIdentifier})?(?:\{configuration.OptionalElementIdentifier})?(.*)", RegexOptions.Compiled);
			var optionalElements = sources.SelectMany(m => m)
				.Where(m => m.Key.StartsWith(configuration.OptionalElementIdentifier))
				.ToArray();
			foreach (var key in optionalElements)
			{
				var matches = regex2.Match(key.Key).Groups;
				var counter = matches.Count;
				if (counter >= 2)
				{
					var newKey = matches[2].Value;
					var source = sources.FirstOrDefault(m => m.ContainsKey(key.Key));
					source?.Remove(key.Key);
					source?.Add(newKey, key.Value);

					if (matches[1].Value == configuration.OptionalElementIdentifier)
						OptionalElements.Add(newKey, key.Value);
				}
			}

			ValidatePageDefinitionForCorrectness(configuration);
		}

		private void ValidatePageDefinitionForCorrectness(IDefinitionConfiguration configuration)
		{
			if (!new[] {"Global", "Scenario"}.Contains(Type))
			{
				var isAUniqueElementDefined = UniqueElements.Any();
				var isAnAwaitableElementDefined = MustAwaitElements.Any();

				if (!isAUniqueElementDefined && !isAnAwaitableElementDefined)
				{
					throw new InvalidDataException($"Neither a Unique Element, designated by a '{configuration.ElementUniquenessIdentifier}', nor an Awaitable Element, designated by a '{configuration.MustAwaitElementLoadIdentifier}', has been specified in the Page Definition file {Name}.ini.  At least one of both types must be specified in the file.");
				}
				else if (!isAUniqueElementDefined)
				{
					throw new InvalidDataException($"A Unique Element, designated by a '{configuration.ElementUniquenessIdentifier}', has not been specified in the Page Definition file {Name}.ini.  At least one globally unique element or unique combination of elements must be designated for each page defined.");
				}
				else if (!isAnAwaitableElementDefined)
				{
					throw new InvalidDataException($"An Awaitable Element, designated by a '{configuration.MustAwaitElementLoadIdentifier}', has not been specified in the Page Definition file {Name}.ini.  At least one element must be defined to indicate when a page has completed loading.");
				}
			}
		}

		public override string ToString()
		{
			return $"{Name} ({Type})";
		}

		#region IDisposable

		// To detect redundant calls
		private bool _disposed;

		~Definition() => Dispose(false);

		public void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// TODO: dispose managed state (managed objects).
				Ids.Clear();
				AutomationIds.Clear();
				AutomationNames.Clear();
				AutomationClassNames.Clear();
				AutomationLinkTexts.Clear();
				AutomationXPaths.Clear();
				AutomationCSSes.Clear();
				AutomationPartialLinkTexts.Clear();
				UniqueElements.Clear();
				MustAwaitElements.Clear();
				OptionalElements.Clear();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			// TODO: set large fields to null.

			_disposed = true;
		}

		// Public implementation of Dispose pattern callable by consumers.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
