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
    public class ValueTupleParameterTest
    {
        [Fact]
        public async Task ValueTupleParameter()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            using (var server = new TestServer(host))
            {
                var client = server.CreateClient();
                var stub = new NetStitchClient("http://localhost/", client).Create<client.IValueTupleParameterTest>();
                await stub.ValueTupleParameterTestAsync( ( a:1, (b:2, c:3)) );
                await stub.ValueTupleParameterTest2Async( (4, 5) );

            }
        }
    }
    public class ValueTupleParameterTests : server.IValueTupleParameterTest
    {

        public (int a, (int b, int c) d) ValueTupleParameterTest((int a, (int b, int c) d) valuetuple)
        {
            Assert.Equal(valuetuple.a, 1);
            Assert.Equal(valuetuple.d.b, 2);
            Assert.Equal(valuetuple.d.c, 3);
            return valuetuple;
        }

        public (int a, int b) ValueTupleParameterTest2((int a, int b) valuetuple)
        {
            Assert.Equal(valuetuple.a, 4);
            Assert.Equal(valuetuple.b, 5);
            return valuetuple;
        }

    }
    namespace client
    {
        [NetStitchContract]
        public interface IValueTupleParameterTest
        {
            [Operation("ValueTupleParameterTest")]
            Task<(int a, (int b, int c) d)> ValueTupleParameterTestAsync(
                (int a, (int b, int c) d) valuetuple
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
            [Operation("ValueTupleParameterFlagTest")]
            Task<(int a, int b)> ValueTupleParameterTest2Async(
                (int a, int b) valuetuple
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
);
        }
    }
    namespace server
    {
        [NetStitchContract]
        public interface IValueTupleParameterTest
        {
            [Operation("ValueTupleParameterTest")]
            (int a, (int b, int c) d) ValueTupleParameterTest(
                (int a, (int b, int c) d) valuetuple
                );
            [Operation("ValueTupleParameterFlagTest")]
            (int a, int b) ValueTupleParameterTest2(
                (int a, int b) valuetuple
                );
        }
    }
}
