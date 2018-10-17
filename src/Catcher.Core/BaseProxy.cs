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
        public Type OriginalTargetType { get; set; }

        /// <summary>
        /// Return the call method
        /// </summary>
        /// <returns></returns>
        public MethodBase GetMethod(object target)
        {
            var methodBase = new StackTrace(new StackFrame(1)).GetFrame(0).GetMethod();
            var currentTargetType = target.GetType();

            return currentTargetType.GetMethod(methodBase.Name, methodBase.GetParameters().Select(n => n.ParameterType).ToArray());
        }
    }
}
