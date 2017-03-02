using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NetStitch.Option;
using SharedInterface;
using System;
using System.Linq;
using SharedProjectValueTuple;
using System.Threading.Tasks;
using NetStitch;
using NetStitch.Server;

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
            app.UseNetStitch(this.GetType());
        }
    }

    public struct Tally : IEcho, IAsyncTest, IOperationContext
    {
        public OperationContext Context { get; set; }

        public async Task<int> TestAsync(int a, int b)
        {
            return await Task.FromResult(a + b);
        }

        MyClass IEcho.Echo(string name, int x, int y, MyEnum e) => new MyClass() { Name = name, Sum = (x + y) * (int)e };

        int IEcho.Sum(int[] array) => array.Sum();

    }
}