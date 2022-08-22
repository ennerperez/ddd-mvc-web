using System.Collections.Generic;

namespace Tests.Abstractions.Interfaces
{
    public interface IAttributeLibrary
    {
        Dictionary<string, object> AttributeLibrary { get; }
        object GetAttributeFromAttributeLibrary(string attributeKey, bool throwException = true);
        void SetAttributeInAttributeLibrary(string attributeKey, object attributeObject);
    }
}
