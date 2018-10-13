[![NuGet](https://img.shields.io/nuget/v/Catcher.Core.svg)](https://www.nuget.org/packages/Catcher.Core)
<p align="center" markdown="1">
  <img src="https://raw.githubusercontent.com/codellion/catcher/master/logo.png" width="300">  
</p>

Is a simple interceptors system for .Net Standard based in Scrutor's decorators.

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

* `AddInterceptor` - It adds an interceptor class to the implementation of a interface and It allow handle the pre and post execution.

See **Examples** below for usage examples.

## Examples

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

Logo designed by from Flaticon.
