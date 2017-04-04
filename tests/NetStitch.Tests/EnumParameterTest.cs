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
    public class EnumParameterTest
    {
        ServerFixture server;
        public EnumParameterTest(ServerFixture server)
        {
            this.server = server;
        }

        [Fact]
        public async Task EnumParameter()
        {
            var stub = server.CreateStub<IEnumParameterTest>();
            await stub.EnumParameterTestAsync(MyEnum.b);
            await stub.EnumFlagParameterTestAsync(MyEnumFlag.f);

        }
    }
}

namespace NetStitch.Tests
{
    using NetStitch.Tests.Server;
    public class EnumParameterServer : IEnumParameterTest
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
}