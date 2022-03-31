using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Contexts;
using Persistence.Interfaces;

namespace Persistence
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
        {
            services.AddDbContext<DefaultContext>(optionsBuilder);
            services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder, Action<T> configureOptions = null)
        {
            services.AddDbContext<DefaultContext>(optionsBuilder);
		    services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence(configureOptions);
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            services.AddDbContext<DefaultContext>();
            services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            //TODO: Make this possible
            //services.AddTransient(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            //services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddRepositoriesFromAssembly(Assembly.GetExecutingAssembly());
            
            return services;
        }

        #region FromAssembly
        
        private static void AddRepositoriesFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
            }
            
            var assembliesToScan = assemblies.Distinct().ToArray();
            ConnectImplementationsToTypesClosing(typeof(IGenericRepository<,>), services, assembliesToScan, false);
            ConnectImplementationsToTypesClosing(typeof(IGenericRepository<>), services, assembliesToScan, false);
        }

        private static void ConnectImplementationsToTypesClosing(Type openRequestInterface, IServiceCollection services, IEnumerable<Assembly> assembliesToScan, bool addIfAlreadyExists)
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();
            foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()))
            {
                var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();
                if (!interfaceTypes.Any()) continue;

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
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                }
            }
        }
        
        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
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
        
        #endregion
        
    }
}
