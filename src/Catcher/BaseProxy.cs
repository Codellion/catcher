using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Catcher
{
    /// <summary>
    /// Base class of proxy services
    /// </summary>
    public class BaseProxy
    {
        /// <summary>
        /// Return the call method
        /// </summary>
        /// <returns></returns>
        public MethodBase GetMethod()
        {
            return new StackTrace(new StackFrame(1)).GetFrame(0).GetMethod();
        }
    }
}
