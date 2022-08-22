using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Helpers
{
    public class StepsHelper : IStepHelper
    {
        private readonly IAutomationContext _automationContext;
        private readonly IAutomationConfiguration _automationConfigurations;

        public IAutomationContext AutomationContext => _automationContext;
        public IAutomationConfiguration AutomationConfigurations => _automationConfigurations;

        public StepsHelper(IAutomationContext automationContext, IAutomationConfiguration automationConfigurations)
        {
            _automationContext = automationContext;
            _automationConfigurations = automationConfigurations;
            
            Initialize();
        }

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

        public List<string> GetColumnValuesFromTable(string columnName, Table dataTable)
        {
            return new List<string>(dataTable.Rows.Select(m => m[columnName]).ToArray());
        }

        public List<string> GetUniqueColumnValuesFromTable(string columnName, Table dataTable)
        {
            return GetColumnValuesFromTable(columnName, dataTable).Distinct().ToList();
        }

        public List<string> GetUniqueColumnValuesFromTableWithoutEmptyStrings(string columnName, Table dataTable)
        {
            return GetColumnValuesFromTable(columnName, dataTable).Distinct().Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
        }

        public bool DoesTableContainColumn(string columnName, Table dataTable)
        {
            return dataTable.ContainsColumn(columnName);
        }

        public string GetCompleteTagFromTagPrefix(string tagPrefix)
        {
            var completeTag = GetCompleteStringFromSubstring(tagPrefix, _automationContext.ScenarioContext.ScenarioInfo.Tags);
            if (string.IsNullOrWhiteSpace(completeTag))
            {
                completeTag = GetCompleteStringFromSubstring(tagPrefix, _automationContext.FeatureContext.FeatureInfo.Tags);
            }

            return completeTag;
        }

        public string GetCompleteStringFromSubstring(string substring, string[] stringArray)
        {
            var regEx = new Regex(@$"{substring}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();
            return tags.Any() ? tags.First().Groups[1].Value : null;
        }

        public bool IsSubstringFoundInStringArray(string substring, string[] stringArray)
        {
            return stringArray.Any(m => m.Contains(substring));
        }

        /* EXCEPTIONS */

        public void ThrowExceptionIfRequiredTagsAreNotInPlace()
        {
            var delimitedListOfMissingRequiredTags = "*";
            foreach (var requiredTagPrefix in _automationConfigurations.RequiredTagPrefixes)
                if (!DoesScenarioOrFeatureContainTagPrefix(requiredTagPrefix))
                    delimitedListOfMissingRequiredTags += ", " + requiredTagPrefix;

            foreach (var requiredTag in _automationConfigurations.RequiredTags)
                if (!DoesScenarioOrFeatureContainTag(requiredTag))
                    delimitedListOfMissingRequiredTags += ", " + requiredTag;

            if (delimitedListOfMissingRequiredTags.Length > 1)
                throw new Exception($"Not all of the required Tags or Tag Prefixes have been placed in the Feature. Please ensure the following Tags or Tag Prefixes exist: {delimitedListOfMissingRequiredTags.Replace("*, ", "")}");
        }
        
        /* SCOPEDS */
        
        public string DetermineEnvironmentTarget()
        {
            var environmentTarget = Environment.GetEnvironmentVariable("Runtime_Environment_Target");
            if (string.IsNullOrWhiteSpace(environmentTarget))
            {
                //Default to the last Environment Tag that was found in the Feature File, either at the Feature or Scenario level
                environmentTarget = GetTagParameterForTagPrefix(Resources.Keywords.EnvironmentTarget);
            }

            return environmentTarget;
        }
        
        public void Initialize()
        {
            ThrowExceptionIfRequiredTagsAreNotInPlace();

            if (!AutomationContext.IsInitialized)
            {
                AutomationContext.AutomationType = GetTagParameterForTagPrefix(Resources.Keywords.AutomationType);
                AutomationContext.PlatformTarget = GetTagParameterForTagPrefix(Resources.Keywords.PlatformTarget);
                AutomationContext.ApplicationTarget = GetTagParameterForTagPrefix(Resources.Keywords.ApplicationTarget);
                AutomationContext.EnvironmentTarget = DetermineEnvironmentTarget();
                AutomationContext.Priority = GetTagParameterForTagPrefix(Resources.Keywords.Priority);

                AutomationContext.TestPlanTarget = GetTagParameterForTagPrefix(Resources.Keywords.TestPlan);
                AutomationContext.TestSuiteTarget = GetTagParameterForTagPrefix(Resources.Keywords.TestSuite);
                AutomationContext.TestCaseTarget = GetTagParameterForTagPrefix(Resources.Keywords.TestCase);
                AutomationContext.IsInitialized = true;
            }
        }
    }
}
