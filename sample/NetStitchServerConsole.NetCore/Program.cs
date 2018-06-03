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
            app.UseMiddleware<NetStitch.Swagger.NetStitchSwaggerMiddleware>();
            app.UseNetStitch(this.GetType(), new NetStitchOption()
            {
                ExceptionHandling = ExceptionHandling.ShowStackTrace,
                Logger = new NetStitch.Logger.NetStitchConsoleLogger(),
                FormatterResolver = MessagePack.Resolvers.StandardResolver.Instance,
            });
        }
    }

    public class Tally :  IOperationContext, SharedInterface.IAsyncTest
    {
        public OperationContext Context { get; set; }

        public Task OutputLogAsync(string message, string message2, MyEnum e, MyEnum2 e2)
        {
            Console.WriteLine("log");
            return Task.CompletedTask;
        }

        public async ValueTask<MyClass> EchoAsync()
        {
            return new MyClass() { Name = "ABCああああいいいいいうううううええええおおおおおあああああいいいいいうううううえええええおおおおおあああああいいいいいうううううえええええおおおお" , Sum = 123 };
        }

        public async ValueTask<int> SumAsync(int[] array)
        {
            return array.Sum();
        }

        public async ValueTask<int> TestAsync(int a, int b, int? c)
        {
            return await Task.FromResult(a + b);
        }

        public async ValueTask<int> TestAsync(int a, int b)
        {
            return a + b;
        }
    }
}