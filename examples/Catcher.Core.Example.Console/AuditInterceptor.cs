using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Catcher.Core;

namespace Catcher.Core.Example.Console
{
    public class AuditInterceptor : IInterceptor
    {
        private DateTime startTime;

        public void PreIntercept(CatcherContext context)
        {
            startTime = DateTime.Now;
        }

        public void PostIntercept(CatcherContext context)
        {
            System.Console.WriteLine(DateTime.Now - startTime);
        }
    }
}
