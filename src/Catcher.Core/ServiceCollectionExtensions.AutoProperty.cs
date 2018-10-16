using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using Catcher.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add property injection to the interface
        /// </summary>
        /// <param name="services">ServiceCollecion</param>
        /// <param name="interfaceType">Interface</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddPropertyInjection(this IServiceCollection services, Type interfaceType)
        {
            var oriTypeDesc = services.FirstOrDefault(n => n.ServiceType.Equals(interfaceType));

            if (oriTypeDesc == null)
            {
                throw new ArgumentException("I must be implemented by a class");
            }

            if (oriTypeDesc.ImplementationType != null)
            {
                var proxyType = AutoPropertyProxyFactory.CreateProxy(interfaceType, oriTypeDesc.ImplementationType);
                services = services.Decorate(interfaceType, proxyType);
            }

            return services;
        }

        /// <summary>
        /// Add property injection to the interface
        /// </summary>
        /// <typeparam name="I">Interface</typeparam>
        /// <param name="services">ServiceCollecion</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddPropertyInjection<I>(this IServiceCollection services)
        {
            var interfaceType = typeof(I);
            return AddPropertyInjection(services, interfaceType);
        }

        /// <summary>
        /// Add property injection to the interface
        /// </summary>
        /// <param name="services">ServiceCollecion</param>
        /// <param name="predicate">Interface match</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddPropertyInjection(this IServiceCollection services,
            Func<ServiceDescriptor, bool> predicate)
        {
            var selTypes = services.Where(predicate);

            selTypes.ToList()
                .ForEach(selType =>
                {
                    services = AddPropertyInjection(services, selType.ServiceType);
                });

            return services;
        }
    }
}
