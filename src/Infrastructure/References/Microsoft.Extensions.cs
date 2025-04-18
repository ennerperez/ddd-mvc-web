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
            public static void ConnectImplementationsToTypesClosing(this IServiceCollection services, Type openRequestInterface, IEnumerable<Assembly> assembliesToScan, bool addIfAlreadyExists, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
                            switch (serviceLifetime)
                            {
                                case ServiceLifetime.Singleton:
                                    services.AddSingleton(@interface, type);
                                    break;
                                case ServiceLifetime.Scoped:
                                    services.AddScoped(@interface, type);
                                    break;
                                default:
                                    services.AddTransient(@interface, type);
                                    break;
                            }
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
                            switch (serviceLifetime)
                            {
                                case ServiceLifetime.Singleton:
                                    services.TryAddSingleton(@interface, type);
                                    break;
                                case ServiceLifetime.Scoped:
                                    services.TryAddScoped(@interface, type);
                                    break;
                                default:
                                    services.TryAddTransient(@interface, type);
                                    break;
                            }
                        }
                    }

                    if (!@interface.IsOpenGeneric())
                    {
                        AddConcretionsThatCouldBeClosed(services, @interface, concretions);
                    }
                }
            }

            internal static void AddConcretionsThatCouldBeClosed(this IServiceCollection services, Type @interface, List<Type> concretions, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            {
                foreach (var type in concretions
                             .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
                {
                    try
                    {
                        switch (serviceLifetime)
                        {
                            case ServiceLifetime.Singleton:
                                services.TryAddSingleton(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                                break;
                            case ServiceLifetime.Scoped:
                                services.TryAddScoped(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                                break;
                            default:
                                services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            private static bool IsOpenGeneric(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;

            private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType) => FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();

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
                            .Where(type => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == templateType))
                    {
                        yield return interfaceType;
                    }
                }
                else
                {
                    var memberInfo = pluggedType.GetTypeInfo().BaseType;
                    if (memberInfo != null && memberInfo.GetTypeInfo().IsGenericType && memberInfo.GetGenericTypeDefinition() == templateType)
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

            private static bool IsConcrete(this Type type) => !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;

            private static void Fill<T>(this IList<T> list, T value)
            {
                if (list.Contains(value))
                {
                    return;
                }

                list.Add(value);
            }

            private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
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

            private static bool IsMatchingWithInterface(this Type handlerType, Type handlerInterface)
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

            private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
            {
                var openInterface = closedInterface.GetGenericTypeDefinition();
                var arguments = closedInterface.GenericTypeArguments;

                var concreteArguments = openConcretion.GenericTypeArguments;
                return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
            }
        }
    }
}
