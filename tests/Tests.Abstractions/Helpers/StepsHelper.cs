#if USING_REQNROLL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin;
using Reqnroll;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Resources;
// ReSharper disable MemberCanBePrivate.Global

namespace Tests.Abstractions.Helpers
{
    public class StepsHelper : IStepHelper
    {
        private readonly IAutomationConfiguration _automationConfigurations;
        private readonly IAutomationContext _automationContext;

        public StepsHelper(IAutomationContext automationContext, IAutomationConfiguration automationConfigurations)
        {
            _automationContext = automationContext;
            _automationConfigurations = automationConfigurations;

            Initialize();
        }

        public IAutomationContext AutomationContext => _automationContext;
        public IAutomationConfiguration AutomationConfiguration => _automationConfigurations;

        public virtual void CaptureTakeScreenshot(object driver, string method = "", bool trace = false)
            => throw new NotImplementedException();

        public bool DoesFeatureContainTag(string tag)
        {
            var result = string.IsNullOrWhiteSpace(tag) || _automationContext.FeatureContext.FeatureInfo.Tags.Contains(tag);
            return result;
        }

        public bool DoesScenarioContainTag(string tag)
        {
            var result = string.IsNullOrWhiteSpace(tag) || _automationContext.ScenarioContext.ScenarioInfo.Tags.Contains(tag);
            return result;
        }

        public bool DoesScenarioOrFeatureContainTag(string tag)
        {
            var result = DoesScenarioContainTag(tag) || DoesFeatureContainTag(tag);
            return result;
        }

        public bool DoesFeatureContainTagPrefix(string tagPrefix)
        {
            var result = IsSubstringFoundInStringArray($"{tagPrefix}(", _automationContext.FeatureContext.FeatureInfo.Tags);
            return result;
        }

        public bool DoesScenarioContainTagPrefix(string tagPrefix)
        {
            var result = IsSubstringFoundInStringArray($"{tagPrefix}(", _automationContext.ScenarioContext.ScenarioInfo.Tags);
            return result;
        }

        public bool DoesScenarioOrFeatureContainTagPrefix(string tagPrefix)
        {
            var result = DoesScenarioContainTagPrefix(tagPrefix) || DoesFeatureContainTagPrefix(tagPrefix);
            return result;
        }

        public string GetTagParameterForTagPrefix(string tagPrefix)
        {
            var result = GetCompleteTagFromTagPrefix(tagPrefix);
            return result;
        }

        public static List<string> GetColumnValuesFromTable(string columnName, Table dataTable) => [..dataTable.Rows.Select(m => m[columnName]).ToArray()];

        public static List<string> GetUniqueColumnValuesFromTable(string columnName, Table dataTable) => GetColumnValuesFromTable(columnName, dataTable).Distinct().ToList();

        public static List<string> GetUniqueColumnValuesFromTableWithoutEmptyStrings(string columnName, Table dataTable) => GetColumnValuesFromTable(columnName, dataTable).Distinct().Where(m => !string.IsNullOrWhiteSpace(m)).ToList();

        public static bool DoesTableContainColumn(string columnName, Table dataTable) => dataTable.ContainsColumn(columnName);

        public string GetCompleteTagFromTagPrefix(string tagPrefix)
        {
            var completeTag = GetCompleteStringFromSubstring(tagPrefix, _automationContext.ScenarioContext.ScenarioInfo.Tags);
            if (string.IsNullOrWhiteSpace(completeTag))
            {
                completeTag = GetCompleteStringFromSubstring(tagPrefix, _automationContext.FeatureContext.FeatureInfo.Tags);
            }

            return completeTag;
        }

        public static string GetCompleteStringFromSubstring(string substring, string[] stringArray)
        {
            var regEx = new Regex(@$"{substring}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();
            return tags.Length != 0 ? tags.First().Groups[1].Value : null;
        }

        public static bool IsSubstringFoundInStringArray(string substring, string[] stringArray) => stringArray.Any(m => m.Contains(substring));

        /* EXCEPTIONS */

        public void ThrowExceptionIfRequiredTagsAreNotInPlace()
        {
            var delimitedListOfMissingRequiredTags = _automationConfigurations.RequiredTagPrefixes.Where(requiredTagPrefix => !DoesScenarioOrFeatureContainTagPrefix(requiredTagPrefix)).Aggregate("*", (current, requiredTagPrefix) => current + (", " + requiredTagPrefix));

            delimitedListOfMissingRequiredTags = _automationConfigurations.RequiredTags.Where(requiredTag => !DoesScenarioOrFeatureContainTag(requiredTag)).Aggregate(delimitedListOfMissingRequiredTags, (current, requiredTag) => current + (", " + requiredTag));

            if (delimitedListOfMissingRequiredTags.Length > 1)
            {
                throw new InvalidTagException($"Not all of the required Tags or Tag Prefixes have been placed in the Feature. Please ensure the following Tags or Tag Prefixes exist: {delimitedListOfMissingRequiredTags.Replace("*, ", "")}");
            }
        }

        /* SCOPES */

        public string DetermineEnvironmentTarget()
        {
            var environmentTarget = Environment.GetEnvironmentVariable("Runtime_Environment_Target");
            if (string.IsNullOrWhiteSpace(environmentTarget))
            {
                //Default to the last Environment Tag that was found in the Feature File, either at the Feature or Scenario level
                environmentTarget = GetTagParameterForTagPrefix(Keywords.EnvironmentTarget);
            }

            return environmentTarget;
        }

        public void Initialize()
        {
            ThrowExceptionIfRequiredTagsAreNotInPlace();

            if (AutomationContext.IsInitialized)
            {
                return;
            }

            this.AutomationContext.AutomationType = GetTagParameterForTagPrefix(Keywords.AutomationType);
            this.AutomationContext.PlatformTarget = GetTagParameterForTagPrefix(Keywords.PlatformTarget);
            this.AutomationContext.ApplicationTarget = GetTagParameterForTagPrefix(Keywords.ApplicationTarget);
            this.AutomationContext.EnvironmentTarget = DetermineEnvironmentTarget();
            this.AutomationContext.Priority = GetTagParameterForTagPrefix(Keywords.Priority);

            this.AutomationContext.TestPlanTarget = GetTagParameterForTagPrefix(Keywords.TestPlan);
            this.AutomationContext.TestSuiteTarget = GetTagParameterForTagPrefix(Keywords.TestSuite);
            this.AutomationContext.TestCaseTarget = GetTagParameterForTagPrefix(Keywords.TestCase);
            this.AutomationContext.IsInitialized = true;
        }
    }
}
#endif
