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

namespace NetStitch.Tests
{
    using NetStitch.Tests.Client;

    [Collection(nameof(ServerCollection))]
    public class ValueTupleParameterTest
    {
        ServerFixture server;
        public ValueTupleParameterTest(ServerFixture server)
        {
            this.server = server;
        }
        [Fact]
        public async Task ValueTupleParameter()
        {
            var stub = server.CreateStub<IValueTupleParameterTest>();
            await stub.ValueTupleParameterTestAsync( ( a:1, (b:2, c:3)) );
            await stub.ValueTupleParameterTest2Async( (4, 5) );

        }
    }
}
namespace NetStitch.Tests
{
    using NetStitch.Tests.Server;
    public class ValueTupleParameterServer : IValueTupleParameterTest
    {

        public async ValueTask<(int a, (int b, int c) d)> ValueTupleParameterTestAsync((int a, (int b, int c) d) valuetuple)
        {
            Assert.Equal(valuetuple.a, 1);
            Assert.Equal(valuetuple.d.b, 2);
            Assert.Equal(valuetuple.d.c, 3);
            return valuetuple;
        }

        public async ValueTask<(int a, int b)> ValueTupleParameterTest2Async((int a, int b) valuetuple)
        {
            Assert.Equal(valuetuple.a, 4);
            Assert.Equal(valuetuple.b, 5);
            return valuetuple;
        }

    }
}