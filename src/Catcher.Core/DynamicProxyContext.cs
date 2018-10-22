using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Catcher.Core
{
    internal sealed class DynamicProxyContext
    {
        private static volatile DynamicProxyContext instance = null;
        private static readonly object padlock = new object();

        private readonly IDictionary<string, ImplementationProxyClass> implementationProxyClasses;

        private DynamicProxyContext()
        {
            implementationProxyClasses = new Dictionary<string, ImplementationProxyClass>();
        }

        public static DynamicProxyContext Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                            instance = new DynamicProxyContext();
                    }
                }

                return instance;
            }
        }

        internal void AddProxy(Type serviceType, Type originalType, Type proxyType)
        {
            GetItem(serviceType, originalType, proxyType);
        }

        internal void AddInterceptor(Type serviceType, Type originalType, Type proxyType, Type interceptorType)
        {
            var implClass = GetItem(serviceType, originalType, proxyType, interceptorType);

            if(implClass != null)
            {
                implClass.Interceptors.Add(interceptorType);
            }
        }

        internal bool HasDynamicProxy(Type serviceType)
        {
            return implementationProxyClasses.ContainsKey(serviceType.FullName);
        }

        internal ImplementationProxyClass GetDynamicProxy(Type serviceType)
        {
            if (HasDynamicProxy(serviceType))
            {
                return implementationProxyClasses[serviceType.FullName];
            }

            return null;
        }

        internal Type[] GetInterceptors(string serviceName)
        {
            if (implementationProxyClasses.ContainsKey(serviceName))
            {
                return implementationProxyClasses[serviceName].Interceptors.ToArray();
            }

            return Array.Empty<Type>();
        }

        private ImplementationProxyClass GetItem(Type serviceType, Type originalType, Type proxyType, Type interceptorType = null)
        {
            ImplementationProxyClass res = null;
            if (implementationProxyClasses.ContainsKey(serviceType.FullName))
            {
                res = implementationProxyClasses[serviceType.FullName];
            }
            else
            {
                ImplementationProxyClass newImp = null;
                if (interceptorType == null)
                {
                    newImp = new ImplementationProxyClass(proxyType, originalType);
                }
                else
                {
                    newImp = new ImplementationProxyClass(proxyType, originalType, interceptorType);
                }
                implementationProxyClasses.Add(serviceType.FullName, newImp);
            }

            return res;
        }
    }
}
