// ReSharper disable once CheckNamespace

namespace System.Reflection
{
    public class ResourceHelper
    {
        public static string GetResourceLookup(Type resourceType, string resourceName)
        {
            if ((resourceType != null) && (resourceName != null))
            {
                PropertyInfo property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (property == null)
                {
                    throw new InvalidOperationException("Resource Type Does Not Have Property");
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException("Resource Property is Not String Type");
                }
                return (string)property.GetValue(null, null);
            }
            return null; 
        }
        
        public static T GetResourceLookup<T>(Type resourceType, string resourceName)
        {
            if ((resourceType != null) && (resourceName != null))
            {
                PropertyInfo property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (property == null)
                {
                    return default(T);
                }

                return (T)property.GetValue(null, null);
            }
            return default(T);
        }
    }
}
