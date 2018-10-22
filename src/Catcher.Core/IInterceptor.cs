using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Catcher.Core
{
    /// <summary>
    /// Interceptor interface
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// It is executed before the method call
        /// </summary>
        /// <param name="context">Interception context</param>
        void Intercept(CatcherContext context);
    }
}
