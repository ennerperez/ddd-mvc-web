using System.Collections.Generic;
using Tests.Abstractions.Enums;
using OpenQA.Selenium;

namespace Tests.Web.Interfaces
{
  public interface IDefinitionService
  {

    IWebElement FindElement(string selector, bool waitForElementToBeDisplayed = false);
    IWebElement FindDynamicElement(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false);

    IWebElement TryFindElement(string selector, int timeout = 1500, int maxAttempts = 5);
    IWebElement TryFindDynamicElement(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5);

    IReadOnlyCollection<IWebElement> FindElements(string selector, bool waitForElementToBeDisplayed = false);
    IReadOnlyCollection<IWebElement> FindDynamicElements(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false);

    IReadOnlyCollection<IWebElement> TryFindElements(string selector, int timeout = 1500, int maxAttempts = 5);
    IReadOnlyCollection<IWebElement> TryFindDynamicElements(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5);

    void SetCurrentPage(string name);
    bool IsOptional(string name);

    KeyValuePair<SelectBy, string> GetStrategy(string selector, string token = "", string name = "");
  }
}
