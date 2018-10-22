# Catcher.Core [![NuGet](https://img.shields.io/nuget/v/Catcher.Core.svg)](https://www.nuget.org/packages/Catcher.Core) [![TravisCI](https://travis-ci.org/Codellion/catcher.svg?branch=master)](https://travis-ci.org/Codellion/catcher)
<p align="center" markdown="1">
  <img src="https://raw.githubusercontent.com/codellion/catcher/master/logo.png" width="300">  
</p>

Is a simple interceptors system for .Net Standard 2.0 based in decorators of [Scrutor](https://github.com/khellang/Scrutor). It also adds a series of features not implemented in the DI container of .net core as property injection.

For to see of the version 1.0 documentation use this [link](https://github.com/Codellion/catcher/blob/1.1.2/README.md).

The new version 2.0 is more powerful and fast, it's has a more simple interface (More similar to the other frameworks) and usage.

## Installation

Install the [Catcher.Core NuGet Package](https://www.nuget.org/packages/Catcher.Core).

### Package Manager Console

```cmd
Install-Package Catcher.Core
```

### .NET Core CLI

```cmd
dotnet add package Catcher.Core
```

## Usage

The library adds a extension method to `IServiceCollection`:

* `AddInterceptor` - It adds an interceptor class to the implementation of an interface and allows to handle the previous and subsequent execution.

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The interceptors must implement the `IInterceptor` interface:

```csharp
  /// <summary>
  /// It is executed before the method call
  /// </summary>
  /// <param name="context">Interception context</param>
  void Intercept(CatcherContext context);
```

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;The `CatcherContext` provides the method info and args in the previous execution and returned value in the final stage. Both the input arguments and the returned value can be modified before being used outside the interceptor. For to realize the execution you must call the `Process` method of the context, and the next interceptor will be called.

* `AddPropertyInjection` - It allows the IoC to inject dependencies through properties instead the class constructor.

See **Examples** below for usage examples.

## Examples

### Registration

The interceptor registration must indicate it in the last place, after register the services. While the register of property injection must be made between the register of services and the register of interceptors.

```csharp
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
```

### Interceptor

```csharp
    public class AuditInterceptor : IInterceptor
    {
        private DateTime startTime;

        public void Intercept(CatcherContext context)
        {
            var startTime = DateTime.Now;
            context.Proceed();
            System.Console.WriteLine(DateTime.Now - startTime);
        }
    }
```

### Service with property injection

```csharp
    public class TestSvc2 : ITestSvc2
    {
        public ITestSvc TestSvc { get; set;}

        public void Test3(int a, int b)
        {
            TestSvc.Test1(a, b);
        }

        public void Test4()
        {
            TestSvc.Test2();
        }
    }
```

Logo designed by from Flaticon.
