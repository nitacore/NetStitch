using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NetStitch.Option;
using SharedInterface;
using System;
using System.Linq;

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
            app.UseNetStitch(this.GetType(), new NetStitchOption() { });
        }
    }

    public class Tally : IEcho
    {
        public MyClass Echo(string name, int x, int y, MyEnum e)
        {
            return new MyClass() { Name = name, Sum = (x + y) * (int)e };
        }

        public int Sum(int[] array) => array.Sum();
    }
}