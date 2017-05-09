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

    public class Tally : IEcho, IAsyncTest, IOperationContext,ISharedInterfaceValueTuple
    {
        public OperationContext Context { get; set; }

        public ValueTask<MyClass> EchoAsync(string name, int x, int y, MyEnum e)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> SumAsync(int[] array)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> TestAsync(int a, int b)
        {
            throw new NotImplementedException();
        }

        async ValueTask<(int sum, int count)> ISharedInterfaceValueTuple.TallyAsync(System.Collections.Generic.IList<(int a, int b)> tes)
        {
            return (tes.Sum(x => x.a + x.b), tes.Count);
        }

    }
}