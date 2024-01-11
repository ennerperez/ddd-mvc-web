// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions
{
    namespace DependencyInjection
    {
        public static class DependencyInjectionExtensions
        {
            public static void ConnectImplementationsToTypesClosing(this IServiceCollection services, Type openRequestInterface, IEnumerable<Assembly> assembliesToScan, bool addIfAlreadyExists)
            {
                var concretions = new List<Type>();
                var interfaces = new List<Type>();
                foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()))
                {
                    var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();
                    if (!interfaceTypes.Any())
                    {
                        continue;
                    }

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }

                    foreach (var interfaceType in interfaceTypes)
                    {
                        interfaces.Fill(interfaceType);
                    }
                }

                foreach (var @interface in interfaces)
                {
                    var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                    if (addIfAlreadyExists)
                    {
                        foreach (var type in exactMatches)
                        {
                            services.AddTransient(@interface, type);
                        }
                    }
                    else
                    {
                        if (exactMatches.Count > 1)
                        {
                            exactMatches.RemoveAll(m => !m.IsMatchingWithInterface(@interface));
                        }

                        foreach (var type in exactMatches)
                        {
                            services.TryAddTransient(@interface, type);
                        }
                    }

                    if (!@interface.IsOpenGeneric())
                    {
                        AddConcretionsThatCouldBeClosed(services, @interface, concretions);
                    }
                }
            }

            internal static void AddConcretionsThatCouldBeClosed(this IServiceCollection services, Type @interface, List<Type> concretions)
            {
                foreach (var type in concretions
                             .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
                {
                    try
                    {
                        services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            private static void Fill<T>(this IList<T> list, T value)
            {
                if (list.Contains(value))
                {
                    return;
                }

                list.Add(value);
            }

        }
    }
}
