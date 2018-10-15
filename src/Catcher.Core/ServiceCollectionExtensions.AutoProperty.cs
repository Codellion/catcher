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
        public static IServiceCollection AddPropertyInjection<I>(this IServiceCollection services)
        {
            var interfaceType = typeof(I);
            var oriTypeDesc = services.FirstOrDefault(n => n.ServiceType.Equals(interfaceType));

            if (oriTypeDesc == null)
            {
                throw new ArgumentException("I must be implemented by a class");
            }

            if (oriTypeDesc.ImplementationType != null)
            {
                var proxyType = ProxyFactory2.CreateProxy(interfaceType, oriTypeDesc.ImplementationType);
                services = services.Decorate(interfaceType, proxyType);
            }

            return services;
        }
    }
}
