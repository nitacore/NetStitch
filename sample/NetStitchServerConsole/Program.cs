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

        public class Tally : SharedInterface.IEcho, SharedInterface.IPerf, IComplexType
        {
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
