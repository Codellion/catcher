using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Catcher.Core
{
    /// <summary>
    /// Base class of proxy services
    /// </summary>
    public class BaseProxy
    {
        private readonly string serviceTypeName;
        private readonly IList<IInterceptor> interceptors;
        private readonly Type targetType;

        public BaseProxy(string serviceTypeName, object target, IServiceProvider serviceProvider)
        {
            this.serviceTypeName = serviceTypeName;
            this.targetType = target.GetType();

            interceptors = new List<IInterceptor>();

            Initialize(serviceProvider);
        }

        /// <summary>
        /// Return the call method
        /// </summary>
        /// <returns></returns>
        public MethodBase GetMethod()
        {
            var methodBase = new StackTrace(new StackFrame(1)).GetFrame(0).GetMethod();
            return targetType.GetMethod(methodBase.Name, methodBase.GetParameters().Select(n => n.ParameterType).ToArray());
        }

        private void Initialize(IServiceProvider serviceProvider)
        {
            var interceptorTypes = DynamicProxyContext.Instance.GetInterceptors(serviceTypeName);
            foreach (var interceptorType in interceptorTypes)
            {
                var interceptor = serviceProvider.GetService(interceptorType);
                if (interceptor != null)
                {
                    interceptors.Add((IInterceptor)interceptor);
                }
            }
        }

        /// <summary>
        /// Call the method
        /// </summary>
        /// <returns></returns>
        public void Execute(CatcherContext interceptionContext)
        {
            interceptionContext.Init(new Queue<IInterceptor>(interceptors));
        }
    }
}
