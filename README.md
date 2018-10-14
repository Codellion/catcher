[![NuGet](https://img.shields.io/nuget/v/Catcher.Core.svg)](https://www.nuget.org/packages/Catcher.Core)
<p align="center" markdown="1">
  <img src="https://raw.githubusercontent.com/codellion/catcher/master/logo.png" width="300">  
</p>

Is a simple interceptors system for .Net Standard 2.0 based in Scrutor's decorators.

## Installation

Install the [Catcher.Core NuGet Package](https://www.nuget.org/packages/Catcher.Core).

### Package Manager Console

```
Install-Package Catcher.Core
```

### .NET Core CLI

```
dotnet add package Catcher.Core
```

## Usage

The library adds a extension method to `IServiceCollection`:

* `AddInterceptor` - It adds an interceptor class to the implementation of an interface and allows to handle the previous and subsequent execution.

The interceptors must implement the `IInterceptor` interface:

```csharp
  /// <summary>
  /// It is executed before the method call
  /// </summary>
  /// <param name="context">Interception context</param>
  void PreIntercept(CatcherContext context);

  /// <summary>
  /// It is executed after the method call
  /// </summary>
  /// <param name="context">Interception context</param>
  void PostIntercept(CatcherContext context);
```

The `CatcherContext` provides the method info and args in the previous execution and returned value in the final stage. It's possible cancel the execution from the `PreIntercept` method setting the **Cancel** property to `True` in the CatcherContext. Both the input arguments and the returned value can be modified before being used outside the interceptor.

See **Examples** below for usage examples.

## Examples

### Registration

```csharp
  var svcProv = new ServiceCollection()
    //.AddTransient<AuditInterceptor>()
    .Scan(scan => scan
        .FromAssemblyOf<Program>()
        .AddClasses(classes => classes.AssignableTo<IInterceptor>())
        .AsSelf()
        .WithTransientLifetime())
    //.AddSingleton<ITestSvc, TestSvc>()
    //.AddSingleton<ITestSvc2, TestSvc2>()
    .Scan(scan => scan
        .FromAssemblyOf<Program>()
        .AddClasses(classes => classes.InNamespaceOf<ITestSvc>())
        .AsImplementedInterfaces()
        .WithSingletonLifetime())                
    //.AddInterceptor<AuditInterceptor, ITestSvc>()
    //.AddInterceptor<AuditInterceptor, ITestSvc2>()
    //.AddInterceptor<AuditInterceptor>(n => n.ServiceType.Name.EndsWith("Svc"))
    .AddInterceptor<AuditInterceptor>(n => typeof(IAuditable).IsAssignableFrom(n.ImplementationType))
    .BuildServiceProvider();
```

### Interceptor
```csharp
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
```

Logo designed by from Flaticon.
