# NetStitch

[![Build status](https://ci.appveyor.com/api/projects/status/9heni02h0fmubqjb?svg=true)](https://ci.appveyor.com/project/nitacore/netstitch)

NetStitch is a Http RPC Framework built on .NetFramework/.NETCore.

It is provides simple and easy Http WebApi Call for C#.

This Framework is composed of two projects NetStitch.Server and NetStitch.Client.

## NetStitch.Server

NetStitch.Server is a Middleware of ASP.NET Core.

### Installation
<pre>PM>Install-Package NetStitch.Server</pre>

#### Package Structure
<pre>
NetStitch.Server Packages
  ├─build
  │  └NetStitch.Server.targets
  └─lib
     └DLLs
</pre>

#### NetStitch.Server.targets
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003" >
   <PropertyGroup>
    <DefineConstants>$(DefineConstants);___server___</DefineConstants>
    <NoWarn>1998</NoWarn>
  </PropertyGroup>
</Project>
```

### Usage
1.Add ASP.Net Core StartUp.
```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseNetStitch(typeof(Startup));
    }
}
```

2.Implementation of "[If Directive Approach](https://github.com/nitacore/Readme#if-directive-approach)" Interface .
```csharp
    public interface Interface : INetStitchContract
    {
        [Operation]
        ValueTask<int> ValueTaskMethodAsync
        (int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
    
    public class MyClass : Interface
    {
        public async ValueTask<int> ValueTaskMethodAsync(int a, int b)
        {
            return a + b;
        }
    }
```

## NetStitch.Client
NetStitch.Client is a RPC Client of NetStitch.Server.

### Installation
<pre>PM>Install-Package NetStitch.Client</pre>
  
### Usage
```csharp
new NetStitch.NetStitchClient(EndPointUrl).Create<Interface>().Method(Parameter);
```

## Sample

See [NetStitch-Sample-CI](https://github.com/nitacore/NetStitch-Sample-CI)

## Feature

### If Directive Approach
NetStitch uses #if preprocessor directive to match the definitions of client and server interfaceses.

Original Method
```csharp
    public interface Interface
    {
        ValueTask<int> ValueTaskMethodAsync(int a, int b);
    }
```

If Directive Approach
```csharp
    public interface Interface : INetStitchContract
    {
        [Operation]
        ValueTask<int> ValueTaskMethodAsync
        (int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
```

#### Pattern List
|ReturnType| Original Method | If Directive Approach |
|----------|---------------------------------|-------------------------------------------|
|Task      | Task MethodAsync();                  |<pre><br>        [Operation]<br>        Task MethodAsync(int a, int b<br>#if !\_\_\_server\_\_\_<br>        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)<br>#endif<br>        );|
|ValueTask\<T\> | ValueTask\<int\> MethodAsync(int a, int b); |<pre><br>        [Operation]<br>        ValueTask<int> MethodAsync(int a, int b<br>#if !\_\_\_server\_\_\_<br>        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)<br>#endif<br>        );|

### Serializer
Request data and Response data are serealize by [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)(LZ4MessagePackSerializer).

Please try to define the type according to MessagePack-CSharp rules.

### Analyzer and CodeFix
NetStitch are Supported Analyzer and Codefix.

#### Usage

1.Get NetStitch.Analyzer from Nuget.

<pre>PM>Install-Package NetStitch.Analyzer</pre>

2.Implement INetStitchContract interface in Interface

3.Select NetStitchCodefix menu item
![codefix](https://cloud.githubusercontent.com/assets/12636774/25947216/6ca5c5a2-3689-11e7-8573-722ed9fb079e.gif)

## License
NetStitch is licensed under MIT. Refer to LICENSE for more information.

This Project is Inspired by [LightNode](https://github.com/neuecc/LightNode).
