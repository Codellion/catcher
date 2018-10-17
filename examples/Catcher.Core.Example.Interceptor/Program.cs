using System;
using Catcher.Core.Example.Interceptor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Catcher.Core.Example.Interceptor
{
    internal class Program
    {
        protected Program() { }

        static void Main(string[] args)
        {
            var svcProv = new ServiceCollection()
                .Scan(scan => scan
                    .FromAssemblyOf<Program>()
                    .AddClasses(classes => classes.AssignableTo<IInterceptor>())
                    .AsSelf()
                    .WithTransientLifetime())
                .Scan(scan => scan
                    .FromAssemblyOf<Program>()
                    .AddClasses(classes => classes.InNamespaceOf<ITestSvc>())
                    .AsMatchingInterface()
                    .WithSingletonLifetime())
                .AddPropertyInjection<ITestSvc2>()
                .AddInterceptor<AuditInterceptor>(n => typeof(IAuditable).IsAssignableFrom(n.ServiceType))
                .BuildServiceProvider();
            
            var testSvc = svcProv.GetRequiredService<ITestSvc>();
            testSvc.Test1(0, 999);
            testSvc.Test2();

            var testSvc2 = svcProv.GetRequiredService<ITestSvc2>();
            testSvc2.Test3(1, 1000);
            testSvc2.Test4();

            Console.WriteLine("PULSE ENTER FOR EXIT...");
            Console.ReadLine();
        }
    }
}
