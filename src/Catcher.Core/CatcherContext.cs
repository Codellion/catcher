using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Catcher.Core
{
    /// <summary>
    /// Interception context
    /// </summary>
    public class CatcherContext
    {
        /// <summary>
        /// Arguments from method call
        /// </summary>
        public object[] Args { get; internal set; }

        /// <summary>
        /// Method intercepted
        /// </summary>
        public MethodBase Method { get; internal set; }

        /// <summary>
        /// Value returned by the method call
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// Instance on which the call is executed.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Original target type
        /// </summary>
        public Type TargetType { get; set; }

        private Queue<IInterceptor> interceptors;

        public CatcherContext(object[] args, MethodBase method, object target)
        {
            Args = args;
            Method = method;
            Target = target;
            TargetType = target.GetType();
        }

        internal void Init(Queue<IInterceptor> interceptors)
        {
            this.interceptors = interceptors;
            Proceed();
        }

        /// <summary>
        /// Execute the intercepted method with the args of the context
        /// </summary>
        public void Proceed()
        {
            if(interceptors != null && interceptors.Count > 0)
            {
                var interceptor = interceptors.Dequeue();
                interceptor.Intercept(this);
            }
            else
            {
                ReturnValue = Method.Invoke(Target, Args);
            }
        }
    }
}
