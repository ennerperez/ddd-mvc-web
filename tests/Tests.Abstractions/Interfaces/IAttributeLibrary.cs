namespace Tests.Abstractions.Interfaces
{
	public interface IAttributeLibrary
	{
		object GetAttribute(string attributeKey, bool throwException = true);
		void SetAttribute(string attributeKey, object attributeObject);
	}
}
