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

            if (oriTypeDesc == null)
            {
                throw new ArgumentException($"The interface {interfaceType.FullName} isn't registered.");
            }

            var dynamicProxy = DynamicProxyContext.Instance.GetDynamicProxy(interfaceType);

            if (dynamicProxy != null)
            {
                DynamicProxyContext.Instance.AddInterceptor(interfaceType, dynamicProxy.OriginalType, 
                    dynamicProxy.ProxyType, interceptorType);

                if (oriTypeDesc.ImplementationType != null)
                {
                    services = services.Decorate(interfaceType, dynamicProxy.ProxyType);
                }
            }
            else
            {
                var proxyType = ProxyFactory.CreateProxy(interfaceType);
                DynamicProxyContext.Instance.AddInterceptor(interfaceType, oriTypeDesc.ImplementationType,
                    proxyType, interceptorType);

                services = services.Decorate(interfaceType, proxyType);
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
