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
        /// Indicate if the interceptor must cancel the method execution
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Instance on which the call is executed.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Original target type
        /// </summary>
        public Type TargetType { get; set; }

        public CatcherContext(object[] args, MethodBase method, object target, Type targetType)
        {
            Args = args;
            Method = method;
            Cancel = false;
            Target = target;
            TargetType = targetType;
        }
    }
}
