using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Catcher.Core.Example.Performance.Services;

namespace Catcher.Core.Example.Performance
{
    public class AuditInterceptor : IInterceptor
    {
        public void Intercept(CatcherContext context)
        {
            var startTime = DateTime.Now;

            context.Proceed();

            if (!context.Method.Name.Equals(nameof(IAuditableCalcService.GetFibonacci)))
            {
                System.Console.WriteLine($"{context.Method.Name} - {DateTime.Now - startTime}");
            }
        }
    }
}
