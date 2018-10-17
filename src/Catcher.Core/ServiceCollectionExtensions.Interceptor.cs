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
        /// Add a interceptor to the interface
        /// </summary>
        /// <param name="services">ServiceCollecion</param>
        /// <param name="interceptorType">Interceptor</param>
        /// <param name="interfaceType">Interface</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddInterceptor(this IServiceCollection services, Type interceptorType, Type interfaceType)
        {
            var oriTypeDesc = services.FirstOrDefault(n => n.ServiceType.Equals(interfaceType));
            Type impType = null;
            Type oriType = null;

            if (oriTypeDesc == null)
            {
                throw new ArgumentException("I must be implemented by a class");
            }

            if (currentImplementations.ContainsKey(interfaceType))
            {
                impType = currentImplementations[interfaceType].CurrentType;
                oriType = currentImplementations[interfaceType].Originalype;
            }
            else if (oriTypeDesc.ImplementationType != null)
            {
                impType = oriTypeDesc.ImplementationType;
                oriType = oriTypeDesc.ImplementationType;
            }

            if (impType != null)
            {
                var proxyType = InterceptorProxyFactory.CreateProxy(interceptorType, interfaceType, impType, oriType);
                services = services.Decorate(interfaceType, proxyType);
                currentImplementations[interfaceType] = new ImplementationProxyClass(proxyType, oriType);
            }

            return services;
        }

        /// <summary>
        /// Add a interceptor to the interface
        /// </summary>
        /// <typeparam name="IN">Interceptor</typeparam>
        /// <typeparam name="I">Interface</typeparam>
        /// <param name="services">ServiceCollecion</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddInterceptor<IN, I>(this IServiceCollection services)
            where IN : IInterceptor
        {
            return AddInterceptor(services, typeof(IN), typeof(I));
        }

        /// <summary>
        /// Add a interceptor to the interfaces
        /// </summary>
        /// <typeparam name="IN">Interceptor</typeparam>
        /// <param name="services">ServiceCollecion</param>
        /// <param name="predicate">Interface match</param>
        /// <returns>ServiceCollecion</returns>
        public static IServiceCollection AddInterceptor<IN>(this IServiceCollection services, 
            Func<ServiceDescriptor, bool> predicate)
            where IN : IInterceptor
        {
            var selTypes = services.Where(predicate);
            var interceptorType = typeof(IN);

            selTypes.ToList()
                .ForEach(selType =>
                {
                    services = AddInterceptor(services, interceptorType, selType.ServiceType);
                });

            return services;
        }
    }
}
