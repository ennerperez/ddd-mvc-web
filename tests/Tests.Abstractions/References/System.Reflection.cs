// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Reflection
{
    public class ResourceHelper
    {
        public static string GetResourceLookup(Type resourceType, string resourceName)
        {
            if ((resourceType != null) && (resourceName != null))
            {
                var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Resource Type Does Not Have Property");
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
                var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (property == null)
                {
                    return default;
                }

                return (T)property.GetValue(null, null);
            }

            return default;
        }
    }

    public static class Extensions
    {
        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type extendedType, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = extendedType.Assembly;
            }

            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                                                       | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
            return query;
        }
    }
}
