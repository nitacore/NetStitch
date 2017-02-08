using NetStitch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using NetStitch.Server;

namespace NetStitch.Tests
{
    public class HttpContextTest
    {
        [Fact]
        public async Task HttpContext()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            using (var server = new TestServer(host))
            {
                var client = server.CreateClient();
                var stub = new NetStitchClient("http://localhost/", client).Create<client.IHttpContextTest>();
                var result = await stub.HttpContextTestAsync("test");
                Assert.Equal(result, "test");
            }
        }
    }
    public class HttpContextTests : server.IHttpContextTest, IHttpContext
    {
        public HttpContext Context { get; set; }

        public string HttpContextTest(string myString)
        {
            Context.Items.Add("a", myString);

            return Context.Items["a"].ToString();
        }
    }
    namespace client
    {
        [NetStitchContract]
        public interface IHttpContextTest
        {
            [Operation("HttpContextTest")]
            Task<string> HttpContextTestAsync(
                string myString,
                System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
        }
    }
    namespace server
    {
        [NetStitchContract]
        public interface IHttpContextTest
        {
            [Operation("HttpContextTest")]
            string HttpContextTest(
                string myString
                );
        }
    }
}
