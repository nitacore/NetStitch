#NetStitch
NetStitch is a Http RPC Framework built on .NetFramework/.NETCore.

This Project is an Alpha Version.

##NetStitch.Server

NetStitch.Server is a Middleware of ASP.NET Core.

###Installation
<pre>PM>Install-Package NetStitch.Server -Pre</pre>

####Package Structure
<pre>
NetStitch.Server Packages
  ├─build
  │  └NetStitch.Server.targets
  └─lib
      └DLL's
</pre>

####NetStitch.Server.targets
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003" >
   <PropertyGroup>
    <DefineConstants>$(DefineConstants);___server___</DefineConstants>
  </PropertyGroup>
</Project>
```



###Usage
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

2.Implementation of "[If Directive](https://github.com/nitacore/Readme#if-directive-approach)" Interface .
```csharp
    [NetStitchContract]
    public interface Interface
    {
        [Operation("6484ffe7-51dc-4a63-9e3f-3582a78d4117")]
#if !___server___
        Task<
#endif
        int
#if !___server___
        >
#endif
#if !___server___
        MethodAsync
#else
        Method
#endif
        ( int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
    
    public class MyClass : Interface
    {
        int Method(int a, int b) => a + b;
    }
```

##NetStitch.Client
NetStitch.Client is a RPC Client of NetStitch.Server.

###Installation
<pre>PM>Install-Package NetStitch.Client -Pre</pre>
  
###Usage
```csharp
new NetStitch.NetStitchClient(EndPointUrl).Create<Interface>().Method(Parameter);
```

##Exsample

See [NetStitch-Sample-CI](https://github.com/nitacore/NetStitch-Sample-CI)

##Feature

###If Directive Approach
NetStitch uses #if preprocessor directive to match the definitions of client and server interfaceses.

Original Method
```csharp
    public interface Interface
    {
       int Method(int a, int b);
    }
```

If Directive Approach
```csharp
    [NetStitchContract]
    public interface Interface
    {
        [Operation("6484ffe7-51dc-4a63-9e3f-3582a78d4117")]
#if !___server___
        Task<
#endif
        int
#if !___server___
        >
#endif
#if !___server___
        MethodAsync
#else
        Method
#endif
        ( int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
```

Highlight with color
![highlight](https://cloud.githubusercontent.com/assets/12636774/22738751/0a064dc0-ee4c-11e6-9358-95b0034d5513.png)

####Pattern List
|ReturnType| Original Method | If Directive Approach |
|----------|---------------------------------|-------------------------------------------|
|void      | void Method(int a, int b);      |<pre>        [Operation("a340e015-66ec-4132-a97e-684d9925abf6")]<br>#if !\_\_\_server\_\_\_<br>        Task<br>#else<br>        void<br>#endif<br>#if !\_\_\_server\_\_\_<br>        MethodAsync<br>#else<br>        Method<br>#endif<br>        ( int a, int b<br>#if !\_\_\_server\_\_\_<br>        , System.Threading.CancellationToken cancellationToken<br>         = default(System.Threading.CancellationToken)<br>#endif<br>        );|
|T         | int Method(int a, int b);       |<pre>        [Operation("6484ffe7-51dc-4a63-9e3f-3582a78d4117")]<br>#if !\_\_\_server\_\_\_<br>        Task<<br>#endif<br>        int<br>#if !\_\_\_server\_\_\_<br>        ><br>#endif<br>#if !\_\_\_server\_\_\_<br>        MethodAsync<br>#else<br>        Method<br>#endif<br>        ( int a, int b<br>#if !\_\_\_server\_\_\_<br>        , System.Threading.CancellationToken cancellationToken<br>         = default(System.Threading.CancellationToken)<br>#endif<br>        );                                           |
|Task      | Task Method();                  |<pre>        [Operation("f911ac4d-7014-46e5-be03-3054ce40ffa6")]<br>        Task MethodAsync(<br>#if !\_\_\_server\_\_\_<br>        System.Threading.CancellationToken cancellationToken<br>         = default(System.Threading.CancellationToken)<br>#endif<br>        );                                           |
|Task\<T\> | Task\<int\> Method(int a, int b); |<pre>        [Operation("ee8f6781-6a83-4e1d-a9ea-8863ccf3ad6a")]<br>        Task\<int\> MethodAsync(int a, int b<br>#if !\_\_\_server\_\_\_<br>        , System.Threading.CancellationToken cancellationToken<br>         = default(System.Threading.CancellationToken)<br>#endif<br>        );                                           |

###Analyzer and CodeFix
NetStitch are Supported Analyzer and Codefix.

![netstitchcodefix](https://cloud.githubusercontent.com/assets/12636774/22738784/3149b278-ee4c-11e6-82bd-5d88b762d212.gif)

(Although it is still incomplete)

###Serializer
NetStitch's request and response are serealized by [ZeroFormatter](https://github.com/neuecc/ZeroFormatter).

Please try to define the type according to ZeroFormatter rules.


##License
NetStitch is licensed under MIT. Refer to LICENSE for more information.

This Project is Inspired by [LightNode](https://github.com/neuecc/LightNode).
