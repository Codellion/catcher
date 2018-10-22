using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Catcher.Core;

namespace Catcher.Core.Example.Interceptor
{
    public class AuditInterceptor : IInterceptor
    {
        public void Intercept(CatcherContext context)
        {
            var startTime = DateTime.Now;
            context.Proceed();
            Console.WriteLine(DateTime.Now - startTime);
        }
    }
}
