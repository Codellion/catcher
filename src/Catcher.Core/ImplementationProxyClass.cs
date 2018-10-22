using System;
using System.Collections.Generic;
using System.Text;

namespace Catcher.Core
{
    internal class ImplementationProxyClass
    {
        internal Type ProxyType { get; set; }
        internal Type OriginalType { get; set; }

        internal ISet<Type> Interceptors { get; set; }

        internal ImplementationProxyClass(Type proxyType, Type originalType)
        {
            ProxyType = proxyType;
            OriginalType = originalType;
            Interceptors = new HashSet<Type>();
        }

        internal ImplementationProxyClass(Type proxyType, Type originalType, Type interceptorType)
        {
            ProxyType = proxyType;
            OriginalType = originalType;
            Interceptors = new HashSet<Type> { interceptorType };
        }
    }
}
