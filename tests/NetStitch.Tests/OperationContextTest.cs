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
    using NetStitch.Tests.Client;

    [Collection(nameof(ServerCollection))]
    public class OperationContextTest
    {

        ServerFixture server;
        public OperationContextTest(ServerFixture server)
        {
            this.server = server;
        }

        [Fact]
        public async Task OperationContext()
        {
            var stub = server.CreateStub<IOperationContextTest>();
            var result = await stub.HttpContextTestAsync("test");
            Assert.Equal("test", result);
        }
    }
}

namespace NetStitch.Tests
{
    using NetStitch.Tests.Server;

    public class OperationContextServer : IOperationContextTest, IOperationContext
    {
        public OperationContext Context { get; set; }

        public async ValueTask<string> HttpContextTestAsync(string myString)
        {
            Context.Items.Add("a", myString);

            return Context.Items["a"].ToString();
        }
    }
}
