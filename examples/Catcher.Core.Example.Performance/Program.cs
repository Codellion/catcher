using System;
using Microsoft.Extensions.DependencyInjection;
using Catcher.Core.Example.Performance.Services;

namespace Catcher.Core.Example.Performance
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
                    .AddClasses(classes => classes.InNamespaceOf<ICalcService>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime())
                .AddInterceptor<AuditInterceptor, IAuditableCalcService>()
                .AddInterceptor<AuditInterceptor, IPerformanceTestService>()
                .BuildServiceProvider();

            var perfSvc = svcProv.GetService<IPerformanceTestService>();

            perfSvc.TestWithoutInterceptor();
            perfSvc.TestWithInterceptor();

            var calcSvc = svcProv.GetService<IAuditableCalcService>();
            var res = calcSvc.GetHypotenuse(
                new Models.PartialTriangle {
                    Catheti1 = 3.5,
                    Catheti2 = 5.5
                });

            System.Console.WriteLine($"Hypotenuse: {res}");

            System.Console.WriteLine("PULSE ENTER FOR EXIT...");
            System.Console.ReadLine();
        }
    }
}
