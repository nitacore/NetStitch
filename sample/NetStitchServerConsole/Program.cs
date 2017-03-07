using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedInterface;
using Microsoft.AspNetCore.Http;
using NetStitch;

namespace NetStitchServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var webHost = new WebHostBuilder()
            .UseWebListener()
            .UseStartup<Startup>()
            .UseUrls("http://localhost:54321")
            .Build();

            webHost.Run();
        }
        public class Startup
        {
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseNetStitch(this.GetType(), new NetStitch.Option.NetStitchOption() { Logger = new NetStitch.Logger.NetStitchConsoleLogger() });
            }
        }

        public class testFilter : NetStitchFilterAttribute
        {
            public testFilter() :base(null) {}
            public testFilter(Func<OperationContext, Task> next) : base(next) {}
            public override async Task Invoke(OperationContext context)
            {
                var original = context.HttpContext.Response.Body;
                using (var buffer = new System.IO.MemoryStream())
                {
                    Console.WriteLine("test");
                    context.HttpContext.Response.Body = buffer;
                    await Next(context).ConfigureAwait(false);
                    context.HttpContext.Response.Body = original;
                    context.HttpContext.Response.ContentLength = buffer.Length;
                    buffer.Position = 0;
                    await buffer.CopyToAsync(original);
                };
            }
        }

        [testFilter()]
        public struct Tally : SharedInterface.IEcho, SharedInterface.IPerf, IComplexType, NetStitch.IOperationContext
        {

            public OperationContext Context { get; set; }

            public MyClass Echo(MyClass myClass)
            {
                return myClass;
            }

            public MyClass Foo(string a, int? x, int[] array)
            {
                
                return new MyClass() { };
            }

            public void Fuga()
            {
                Console.WriteLine("fuga");
            }

            public Task<int> HogeAsync(string a, int x, MyEnum e = MyEnum.B)
            {
                return Task.FromResult(1);
            }

            public int Sum(int[] array) => array.Sum();

            public Task VoidTaskAsync()
            {
                return Task.Delay(100);
            }

            MyClass IEcho.Echo(string name, int x, int y, MyEnum e)
            {
                return new MyClass() { Name = name, Sum = (x + y) * (int)e };
            }
        }
    }
}
