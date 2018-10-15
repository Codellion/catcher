using System;
using Catcher.Core.Example.AutoProperty.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Catcher.Core.Example.AutoProperty
{
    internal class Program
    {
        protected Program() { }

        static void Main(string[] args)
        {
            var svcProv = new ServiceCollection()
                .Scan(scan => scan
                    .FromAssemblyOf<Program>()
                    .AddClasses(classes => classes.InNamespaceOf<ICarService>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime())
                .AddPropertyInjection<ICarService>()
                .BuildServiceProvider();

            var carSvc = svcProv.GetService<ICarService>();
            carSvc.CheckSystem();

            System.Console.WriteLine("PULSE ENTER FOR EXIT...");
            System.Console.ReadLine();
        }
    }
}
