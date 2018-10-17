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
        internal class ImplementationProxyClass
        {
            public Type CurrentType { get; set; }
            public Type Originalype { get; set; }

            public ImplementationProxyClass(Type currentType, Type originalype)
            {
                CurrentType = currentType;
                Originalype = originalype;
            }
        }

        private readonly static IDictionary<Type, ImplementationProxyClass> currentImplementations = new Dictionary<Type, ImplementationProxyClass>();
    }
}
