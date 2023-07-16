using System.Collections.Generic;
using Tests.Abstractions.Enums;

namespace Tests.Abstractions.Interfaces
{
    public interface IDefinitionService<TElement>
    {

        TElement FindElement(string selector, bool waitForElementToBeDisplayed = false);
        TElement FindDynamicElement(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false);

        TElement TryFindElement(string selector, int timeout = 1500, int maxAttempts = 5);
        TElement TryFindDynamicElement(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5);

        IReadOnlyCollection<TElement> FindElements(string selector, bool waitForElementToBeDisplayed = false);
        IReadOnlyCollection<TElement> FindDynamicElements(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false);

        IReadOnlyCollection<TElement> TryFindElements(string selector, int timeout = 1500, int maxAttempts = 5);
        IReadOnlyCollection<TElement> TryFindDynamicElements(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5);

        void SetCurrentPage(string name);
        bool IsOptional(string name);

        KeyValuePair<SelectBy, string> GetStrategy(string selector, string token = "", string name = "");
    }
}
