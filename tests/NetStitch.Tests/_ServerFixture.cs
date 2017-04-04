using System;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace NetStitch.Tests
{
    [CollectionDefinition(nameof(ServerCollection))]
    public class ServerCollection : ICollectionFixture<ServerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class ServerFixture : IDisposable
    {
        TestServer server;

        public ServerFixture()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            server = new TestServer(host);
        }
        public T CreateStub<T>()
            where T : INetStitchContract
        {
            return new NetStitchClient("http://localhost", server.CreateClient()).Create<T>();
        }

        public void Dispose()
        {
            server.Dispose();
        }
    }
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.Use(async (httpContext, next) =>
            {
                var ms = httpContext.Request.Body as System.IO.MemoryStream;
                if (ms != null)
                {
                    var field = typeof(System.IO.MemoryStream).GetTypeInfo().GetField("_exposable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
#if NETCOREAPP1_0
                    field.SetValue(ms, true);
#else
                    field.SetValue(ms, true, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null);
#endif

                }
                await next();
            });
            app.UseNetStitch(new[] { typeof(Startup).GetTypeInfo().Assembly, typeof(Server.IBasicParameterTest).GetTypeInfo().Assembly }, new Option.NetStitchOption());
        }
    }
}
