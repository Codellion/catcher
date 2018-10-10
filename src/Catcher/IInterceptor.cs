using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Catcher
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
        void PreIntercept(CatcherContext context);

        /// <summary>
        /// It is executed after the method call
        /// </summary>
        void PostIntercept();
    }
}
