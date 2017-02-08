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
    public class EnumParameterTest
    {
        [Fact]
        public async Task EnumParameter()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            using (var server = new TestServer(host))
            {
                var client = server.CreateClient();
                var stub = new NetStitchClient("http://localhost/", client).Create<client.IEnumParameterTest>();
                await stub.EnumParameterTestAsync(MyEnum.b);
                await stub.EnumParameterFlagTestAsync(MyEnumFlag.f);

            }
        }
    }
    public class EnumParameterTests : server.IEnumParameterTest
    {
        public MyEnumFlag EnumFlagParameterTest(MyEnumFlag myEnum)
        {
            return myEnum;
        }

        public MyEnum EnumParameterTest(MyEnum myEnum)
        {
            return myEnum;
        }
    }
    namespace client
    {
        [NetStitchContract]
        public interface IEnumParameterTest
        {
            [Operation("EnumParameterTest")]
            Task<MyEnum> EnumParameterTestAsync(
                MyEnum myEnum
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
            [Operation("EnumParameterFlagTest")]
            Task<MyEnum> EnumParameterFlagTestAsync(
                MyEnumFlag myEnum
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
);
        }
    }
    namespace server
    {
        [NetStitchContract]
        public interface IEnumParameterTest
        {
            [Operation("EnumParameterTest")]
            MyEnum EnumParameterTest(
                MyEnum myEnum
                );
            [Operation("EnumParameterFlagTest")]
            MyEnumFlag EnumFlagParameterTest(
                MyEnumFlag myEnum
                );
        }
    }

    public enum MyEnum
    {
        a,
        b,
        c
    }

    [Flags]
    public enum MyEnumFlag
    {
        a,
        b,
        c,
        d,
        e,
        f
    }

}
