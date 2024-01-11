using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;

#pragma warning disable CS8632

namespace System.Reflection
{
    public static class EntityExtensions
    {
        public static bool IsSoftDelete(this IEntity entity)
        {
            return typeof(ISoftDelete).IsAssignableFrom(entity.GetType());
        }
    }

    public static class AssemblyExtensions
    {
        private static string s_copyright;
        public static string Copyright(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_copyright))
            {
                s_copyright = @this.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true).OfType<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;
            }

            return s_copyright;
        }

        private static string s_product;
        public static string Product(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_product))
            {
                s_product = @this.GetCustomAttributes(typeof(AssemblyProductAttribute), true).OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;
            }

            return s_product;
        }

        private static string s_company;
        public static string Company(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_company))
            {
                s_company = @this.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).OfType<AssemblyCompanyAttribute>().FirstOrDefault()?.Company;
            }

            return s_company;
        }

        private static string s_title;
        public static string Title(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_title))
            {
                s_title = @this.GetCustomAttributes(typeof(AssemblyTitleAttribute), true).OfType<AssemblyTitleAttribute>().FirstOrDefault()?.Title;
            }

            return s_title;
        }

        private static string s_description;
        public static string Description(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_description))
            {
                s_description = @this.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true).OfType<AssemblyDescriptionAttribute>().FirstOrDefault()?.Description;
            }

            return s_description;
        }

        private static Version? s_version;
        public static Version? Version(this Assembly @this)
        {
            if (s_version == null)
            {
                var versionString = @this.GetCustomAttributes(typeof(AssemblyVersionAttribute), true).OfType<AssemblyVersionAttribute>().FirstOrDefault()?.Version;
                _ = System.Version.TryParse(versionString, out s_version);
            }

            return s_version;
        }

        private static Version? s_fileVersion;
        public static Version? FileVersion(this Assembly @this)
        {
            if (s_fileVersion == null)
            {
                var fileVersionString = @this.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).OfType<AssemblyFileVersionAttribute>().FirstOrDefault()?.Version;
                _ = System.Version.TryParse(fileVersionString, out s_fileVersion);
            }

            return s_version;
        }

        private static string s_informationalVersion;
        public static string InformationalVersion(this Assembly @this)
        {
            if (string.IsNullOrWhiteSpace(s_informationalVersion))
            {
                s_informationalVersion = @this.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion;
            }

            return s_informationalVersion;
        }
    }

    public static class Extensions
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> FindInterfacesThatClosesCore(this Type pluggedType, Type templateType)
        {
            if (pluggedType == null)
            {
                yield break;
            }

            if (!pluggedType.IsConcrete())
            {
                yield break;
            }

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else
            {
                var memberInfo = pluggedType.GetTypeInfo().BaseType;
                if (memberInfo != null && memberInfo.GetTypeInfo().IsGenericType && (memberInfo.GetGenericTypeDefinition() == templateType))
                {
                    yield return pluggedType.GetTypeInfo().BaseType;
                }
            }

            if (pluggedType.GetTypeInfo().BaseType == typeof(object))
            {
                yield break;
            }

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        public static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        public static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null)
            {
                return false;
            }

            if (pluggedType == pluginType)
            {
                return true;
            }

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        public static bool IsMatchingWithInterface(this Type handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }

        public static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }
    }
}
