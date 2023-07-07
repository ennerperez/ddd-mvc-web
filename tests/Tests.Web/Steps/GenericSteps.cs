using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using TechTalk.SpecFlow;
using Test.Framework.Extended;
using Tests.Abstractions.Interfaces;
using Tests.Web.Interfaces;
// ReSharper disable UnusedParameter.Local

namespace Tests.Web.Steps
{
    [Binding]
    public class GenericSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly IAutomationContext _automationContext;
        private readonly IDefinitionService _definitionService;
        private readonly IStepHelper _stepsHelper;

        public GenericSteps(IAutomationConfiguration automationConfiguration, IAutomationContext automationContext, IStepHelper stepsHelper, IDefinitionService definitionService)
        {
            _automationContext = automationContext;
            _definitionService = definitionService;
            _stepsHelper = stepsHelper;
        }

        private static IWebDriver Driver => Program.Driver;
        private static int MaxAttempts => Program.Configuration?.GetValue<int>("Timeouts:MaxAttempts") ?? 1;

        private static int Timeout => Program.Configuration?.GetValue<int>("Timeouts:Timeout") ?? 3000;

        #region Privates

        private static int GetElementIndex(string position, IEnumerable<IWebElement> elements)
        {
            int? result = null;
            var count = elements.Count();
            if (!string.IsNullOrWhiteSpace(position))
            {
                var pos = position.Trim().ToLower();
                if (pos == "first")
                {
                    result = 0;
                }
                else if (pos == "second")
                {
                    result = 1;
                }
                else if (pos == "third")
                {
                    result = 2;
                }
                else if (pos == "last")
                {
                    result = count - 1;
                }
                else
                {
                    int index;
                    int.TryParse(position, out index);
                    result = index;
                }
            }

            if (result == null || result < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return result.Value;
        }

        private void AtThePage(string name, [CallerMemberName] string method = "")
        {
            try
            {
                _definitionService.SetCurrentPage(name);
            }
            catch (Exception ex)
            {
                throw new InvalidElementStateException($"Page [{name}] is not displayed", ex);
            }
        }

        private void NotAtThePage(string name, [CallerMemberName] string method = "")
        {
            try
            {
                _definitionService.SetCurrentPage(null);
            }
            catch (Exception ex)
            {
                throw new InvalidElementStateException($"Page [{name}] is still displayed", ex);
            }
        }

        private void TapElement(string name, string type, [CallerMemberName] string method = "")
        {
            var element = _definitionService.TryFindElement(name, Timeout, MaxAttempts);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            element.Click();
        }

        private void TapElementAt(string name, string position, string type, [CallerMemberName] string method = "")
        {
            var elements = _definitionService.TryFindElements(name, Timeout, MaxAttempts);
            if (elements == null || !elements.Any())
            {
                throw new NoSuchElementException($"Elements [{name}] are not reachable");
            }

            var index = GetElementIndex(position, elements);
            elements.ElementAt(index).Click();
        }

        private void TapDynamicElement(string name, string token = "", string type = "item", [CallerMemberName] string method = "")
        {
            var element = _definitionService.TryFindDynamicElement(name, token, type, Timeout, MaxAttempts);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            element.Click();
        }

        private void FillForm(Table table, string keyName = "field", string valueName = "value", [CallerMemberName] string method = "")
        {
            foreach (var row in table.Rows)
            {
                if (_automationContext.ScreenshotConfiguration.BeforeStep)
                {
                    _stepsHelper.CaptureTakeScreenshot(Driver, method);
                }

                var key = "";
                var value = "";
                if (row.TryGetValue(keyName, out var value1))
                {
                    key = value1;
                }

                if (row.TryGetValue(valueName, out var value2))
                {
                    value = value2;
                }

                if (!string.IsNullOrWhiteSpace(key))
                {
                    var entry = _definitionService
                        .TryFindElement(key, Timeout, MaxAttempts);

                    if (entry.IsOptional())
                    {
                        continue;
                    }

                    if (entry != null)
                    {
                        entry.Click();
                        entry.SendKeys(value);
                    }
                }

                if (_automationContext.ScreenshotConfiguration.AfterStep)
                {
                    _stepsHelper.CaptureTakeScreenshot(Driver, method);
                }
            }
        }
        private void ElementAppeared(string name, string type, [CallerMemberName] string method = "")
        {
            _definitionService.FindElement(name, true);
        }

        private bool ElementVisible(string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            result = !element.IsOptional() ? element.Displayed : true;
            return result;
        }

        private bool ElementEnabled(string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            result = !element.IsOptional() ? element.Enabled : true;
            return result;
        }

        private bool ElementEnabledAt(string position, string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var elements = _definitionService.TryFindElements(name, Timeout, MaxAttempts);
            if (elements == null || !elements.Any())
            {
                throw new NoSuchElementException($"Elements [{name}] are not reachable");
            }

            var index = GetElementIndex(position, elements);
            result = elements.ElementAt(index).Enabled;

            return result;
        }

        private bool ElementChecked(string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            result = !element.IsOptional() ? element.GetChecked() : true;
            return result;
        }

        private bool ElementCheckedAt(string position, string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var elements = _definitionService.TryFindElements(name, Timeout, MaxAttempts);
            if (elements == null || !elements.Any())
            {
                throw new NoSuchElementException($"Elements [{name}] are not reachable");
            }

            var index = GetElementIndex(position, elements);
            result = elements.ElementAt(index).GetChecked();
            return result;
        }

        #endregion

        #region Publics

        //TODO: Fix this method
        [Obsolete("Not working")]
        private bool ElementSelected(string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            result = !element.IsOptional() ? element.Selected : true;
            return result;
        }

        private bool ElementEmpty(string name, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            result = !element.IsOptional() ? element.Text == string.Empty : true;
            return result;
        }

        //TODO: Move to another location?
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool KeyPress(string key, [CallerMemberName] string method = "")
        {
            var keyChar = "";
            switch (key.ToLower())
            {
                case "enter":
                    keyChar = Keys.Enter;
                    break;
                case "tab":
                    keyChar = Keys.Tab;
                    break;
                case "esc":
                    keyChar = Keys.Escape;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(keyChar))
            {
                new Actions(Driver).SendKeys(keyChar).Perform();
                return true;
            }

            return false;
        }

        //TODO: Move to another location?
        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SetTextTo(string name, string text, string type, [CallerMemberName] string method = "")
        {
            bool result;
            var element = _definitionService.TryFindElement(name);
            if (element == null)
            {
                throw new ElementNotInteractableException($"Element [{name}] is not reachable");
            }

            element.SendKeys(text);
            //TODO: Validate if the text was entered
            result = !element.IsOptional() ? element.Text == text : true;
            //result = element.GetText() == text;
            return result;
        }

        //TODO: Move to another location?
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool SetText(string text, string type, [CallerMemberName] string method = "")
        {
            try
            {
                new Actions(Driver).SendKeys(text).Perform();
                return true;
            }
            catch (Exception ex)
            {
                throw new ElementNotInteractableException($"Unabled to enter '{text}' in the current field", ex);
            }
        }

        #region Generic Steps

        [When(@"the user (should |)?(?:taps on|clicks|selects|focuses on) the ""(.*)"" (item|button|tab|icon|option|switch|.*).*?")]
        public void UserTapOn(string should, string name, string type = "item")
        {
            var isShould = !string.IsNullOrWhiteSpace(should);
            try
            {
                TapElement(name, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                if (isShould)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail(e);
                }
            }
        }

        [When(@"the user (?:taps on|clicks|selects|focuses on) the (first|second|third|last).*?""(.*)"" ?(item|button|tab|icon|option|switch|.*)?.*?")]
        public void UserTapOnAt(string position, string name, string type)
        {
            try
            {
                TapElementAt(name, position, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [When("the user fills in the following form")]
        public void UserFillInTheFollowingForm(Table table)
        {
            try
            {
                FillForm(table);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the user (is|should be) at the ""(.*)"" page")]
        [Then(@"the user (is|should be) at the ""(.*)"" page")]
        public void UserAtThePage(string option, string name)
        {
            var isShouldBe = option == "should be";
            try
            {
                AtThePage(name);
                Assert.Pass();
            }
            catch (Exception e)
            {
                if (isShouldBe)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail(e);
                }
            }
        }

        [Given(@"the user (?:is not at|leave) the ""(.*)"" page")]
        [Then(@"the user (?:is not at|leave) the ""(.*)"" page")]
        [Then(@"the user (?:should|must) (?:not be at|leave) the ""(.*)"" page")]
        public void UserNotAtThePage(string name)
        {
            try
            {
                NotAtThePage(name);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) (should|must) appeared")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) (should|must) appeared")]
        public void TheElementShouldAppeared(string name, string type, string option)
        {
            var isShouldBe = option == "should";
            try
            {
                ElementAppeared(name, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                if (isShouldBe)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail(e);
                }
            }
        }

        [Given(@"the (item|button|tab|icon|option|switch|.*) (?:should|must) displayed ""(.*)"".*?")]
        [Then(@"the (item|button|tab|icon|option|switch|.*) (?:should|must) displayed ""(.*)"".*?")]
        public void TheElementShouldDisplayed(string type, string name = "item")
        {
            try
            {
                ElementAppeared(name, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (visible|hidden)")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (visible|hidden)")]
        public void TheElementIsVisibleOrHidden(string name, string type, string state)
        {
            try
            {
                var result = ElementVisible(name, type);
                if (state == "visible" && !result || state == "hidden" && result)
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (enabled|disabled)")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (enabled|disabled)")]
        public void TheElementIsEnabledOrDisabled(string name, string type, string state)
        {
            try
            {
                var result = ElementEnabled(name, type);
                if (state == "enabled" && !result || state == "disabled" && result)
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the (first|second|third|last) ""(.*)"" (item|button|tab|icon|option|switch|.*) is (enabled|disabled)")]
        [Then(@"the (first|second|third|last) ""(.*)"" (item|button|tab|icon|option|switch|.*) is (enabled|disabled)")]
        public void TheElementIsEnabledOrDisabledAt(string position, string name, string type, string state)
        {
            try
            {
                var result = ElementEnabledAt(position, name, type);
                if (state == "enabled" && !result || state == "disabled" && result)
                {
                    throw new InvalidElementStateException($"The {position} {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (checked|unchecked|on|off|true|false)")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (checked|unchecked|on|off|true|false)")]
        public void TheElementIsCheckedOrUnchecked(string name, string type, string state)
        {
            try
            {
                var l1 = new[] { "checked", "on", "true" };
                var l2 = new[] { "unchecked", "off", "false" };
                var result = ElementChecked(name, type);
                if ((l1.Contains(state) && !result) || (l2.Contains(state) && result))
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the (first|second|third|last) ""(.*)"" (item|button|tab|icon|option|switch|.*) is (checked|unchecked|on|off|true|false)")]
        [Then(@"the (first|second|third|last) ""(.*)"" (item|button|tab|icon|option|switch|.*) is (checked|unchecked|on|off|true|false)")]
        public void TheElementIsCheckedOrUncheckedAt(string position, string name, string type, string state)
        {
            try
            {
                var l1 = new[] { "checked", "on", "true" };
                var l2 = new[] { "unchecked", "off", "false" };
                var result = ElementCheckedAt(position, name, type);
                if ((l1.Contains(state) && !result) || (l2.Contains(state) && result))
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        //TODO: Fix this method
        [Obsolete("Not working")]
        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (selected|unselected)")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (selected|unselected)")]
        public void ElementIsSelectedOrUnselected(string name, string type, string state)
        {
            try
            {
                var result = ElementSelected(name, type);
                if (state == "selected" && !result || state == "unselected" && result)
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [Given(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (empty|not empty)")]
        [Then(@"the ""(.*)"" (item|button|tab|icon|option|switch|.*) is (empty|not empty)")]
        public void ElementIsEmptyOrNotEmpty(string name, string type, string state)
        {
            try
            {
                var result = ElementEmpty(name, type);
                if (state == "empty" && !result || state == "not empty" && result)
                {
                    throw new InvalidElementStateException($"Element {name} is not '{state}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [When(@"the user (?:enters|writes|types) ""(.*)"" into the currently active (field|entry|.*)")]
        public static void UserEntersIntoTheCurrentlyActiveField(string text, string type)
        {
            try
            {
                SetText(text, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [When(@"the user (?:enters|writes|types) ""(.*)"" in the ""(.*)"" (field|entry|.*)")]
        public void UserEntersInTheField(string text, string name, string type)
        {
            try
            {
                SetTextTo(name, text, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [When(@"the user presses ([Ee]nter|[Tt]ab|[Ee]sc) on the keyboard")]
        public static void UserPressesOnTheKeyboard(string key)
        {
            try
            {
                KeyPress(key);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        #endregion

        [Obsolete("dynamically-named")]
        [Given(@"the user is at the ""(.*)""(?:.*) with title ""(.*)""")]
        [Then(@"the user is at the ""(.*)""(?:.*) with title ""(.*)""")]
        public void UserAtScreenWithTitle(string name, string expectedTitle)
        {
            try
            {
                _definitionService.TryFindElement(name, Timeout, MaxAttempts);
                var element = _definitionService.FindElement(name);
                //var actualTitle = element.GetText();
                var actualTitle = element.Text;
                if (actualTitle != expectedTitle)
                {
                    throw new InvalidElementStateException($"This screen is not '{expectedTitle}' but actually '{actualTitle}'");
                }

                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        [When(@"the user will wait (.*) (seconds|minutes|hours|days)"), Scope(Tag = "ExplicitWaitTimes")]
        public static void ThenWait(int time, string interval)
        {
            var timeSpan = TimeSpan.Zero;
            switch (interval)
            {
                case "seconds":
                    timeSpan = TimeSpan.FromSeconds(time);
                    break;
                case "minutes":
                    timeSpan = TimeSpan.FromMinutes(time);
                    break;
                case "hours":
                    timeSpan = TimeSpan.FromHours(time);
                    break;
                case "days":
                    timeSpan = TimeSpan.FromDays(time);
                    break;
            }

            Thread.Sleep(timeSpan);
            Assert.Pass();
        }

        [When(@"the user (?:taps on|clicks|selects|says) the dynamically-named ""(.*)"" (item|button|tab|icon|option|switch|.*)")]
        public void UserTapTheDynamicallyNamed(string name, string type = "item")
        {
            try
            {
                var token = $"{type.ToUpperInvariant()}_TEXT";
                TapDynamicElement(name, token, type);
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e);
            }
        }

        #region When Steps

        [When(@"the User waits for (.*) second"), Scope(Tag = "ExplicitWaitTimes")]
        [When(@"the User waits for (.*) seconds"), Scope(Tag = "ExplicitWaitTimes")]
        public static void WhenTheUserWaitsForSeconds(int numberOfSeconds)
        {
            Thread.Sleep(numberOfSeconds * 1000);
        }

        #endregion

        #endregion

    }
}
