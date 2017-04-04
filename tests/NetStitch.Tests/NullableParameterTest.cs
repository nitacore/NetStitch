using System;
using System.Linq;
using Xunit;
using NetStitch;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace NetStitch.Tests
{
    using NetStitch.Tests.Client;

    [Collection(nameof(ServerCollection))]
    public class NullableParameterTest
    {

        ServerFixture server;
        public NullableParameterTest(ServerFixture server)
        {
            this.server = server;
        }

        [Fact]
        public async Task NullableParameter()
        {
            var stub = server.CreateStub<INullableParameterTest>();

            await stub.NullableParameterTest1Async(
                "myString",
                sbyte.MaxValue,
                byte.MaxValue,
                short.MaxValue,
                int.MaxValue,
                long.MaxValue,
                ushort.MaxValue,
                uint.MaxValue,
                ulong.MaxValue,
                float.MaxValue,
                double.MaxValue,
                decimal.MaxValue,
                true,
                new DateTime(2000, 1, 1).ToUniversalTime(),
                DateTimeOffset.MaxValue,
                new TimeSpan(1, 1, 1, 1, 1),
                'c'
                );

            await stub.NullableParameterTest2Async(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            await stub.NullableParameterTest3Async();
                
        }
    }
}

namespace NetStitch.Tests
{
    using NetStitch.Tests.Server;
    public class NullableParameterServer : INullableParameterTest
    {
        public int NullableParameterTest1(string myString, sbyte? mySByte, byte? myByte, short? myInt16, int? myInt32, long? myInt64, ushort? myUInt16, uint? myUInt32, ulong? myUInt64, float? mySingle, double? myDouble, decimal? myDecimal, bool? myBoolean, DateTime? myDateTime, DateTimeOffset? myDateTimeOffset, TimeSpan? myTimeSpan, char? myChar)
        {
            Assert.Equal(myString, "myString");
            Assert.Equal(mySByte, sbyte.MaxValue);
            Assert.Equal(myByte, byte.MaxValue);
            Assert.Equal(myInt16, short.MaxValue);
            Assert.Equal(myInt32, int.MaxValue);
            Assert.Equal(myInt64, long.MaxValue);
            Assert.Equal(myUInt16, ushort.MaxValue);
            Assert.Equal(myUInt32, uint.MaxValue);
            Assert.Equal(myUInt64, ulong.MaxValue);
            Assert.Equal(mySingle, float.MaxValue);
            Assert.Equal(myDouble, double.MaxValue);
            Assert.Equal(myDecimal, decimal.MaxValue);
            Assert.Equal(myBoolean, true);
            Assert.Equal(myDateTime, new DateTime(2000, 1, 1).ToUniversalTime());
            Assert.Equal(myDateTimeOffset, DateTimeOffset.MaxValue);
            Assert.Equal(myTimeSpan, new TimeSpan(1, 1, 1, 1, 1));
            Assert.Equal(myChar, 'c');
            return 0;
        }
        public int NullableParameterTest2(string myString, sbyte? mySByte, byte? myByte, short? myInt16, int? myInt32, long? myInt64, ushort? myUInt16, uint? myUInt32, ulong? myUInt64, float? mySingle, double? myDouble, decimal? myDecimal, bool? myBoolean, DateTime? myDateTime, DateTimeOffset? myDateTimeOffset, TimeSpan? myTimeSpan, char? myChar)
        {
            Assert.Null(myString);
            Assert.Null(mySByte);
            Assert.Null(myByte);
            Assert.Null(myInt16);
            Assert.Null(myInt32);
            Assert.Null(myInt64);
            Assert.Null(myUInt16);
            Assert.Null(myUInt32);
            Assert.Null(myUInt64);
            Assert.Null(mySingle);
            Assert.Null(myDouble);
            Assert.Null(myDecimal);
            Assert.Null(myBoolean);
            Assert.Null(myDateTime);
            Assert.Null(myDateTimeOffset);
            Assert.Null(myTimeSpan);
            Assert.Null(myChar);
            return 0;
        }
        public int NullableParameterTest3(string myString = "myString", sbyte? mySByte = sbyte.MaxValue, byte? myByte = 255, short? myInt16 = short.MaxValue, int? myInt32 = int.MaxValue, long? myInt64 = long.MaxValue, ushort? myUInt16 = ushort.MaxValue, uint? myUInt32 = uint.MaxValue, ulong? myUInt64 = ulong.MaxValue, float? mySingle = float.MaxValue, double? myDouble = double.MaxValue, decimal? myDecimal = decimal.MaxValue, bool? myBoolean = true, DateTime? myDateTime = default(DateTime?), DateTimeOffset? myDateTimeOffset = default(DateTimeOffset?), TimeSpan? myTimeSpan = default(TimeSpan?), char? myChar = 'c')
        {
            Assert.Equal(myString, "myString");
            Assert.Equal(mySByte, sbyte.MaxValue);
            Assert.Equal(myByte, byte.MaxValue);
            Assert.Equal(myInt16, short.MaxValue);
            Assert.Equal(myInt32, int.MaxValue);
            Assert.Equal(myInt64, long.MaxValue);
            Assert.Equal(myUInt16, ushort.MaxValue);
            Assert.Equal(myUInt32, uint.MaxValue);
            Assert.Equal(myUInt64, ulong.MaxValue);
            Assert.Equal(mySingle, float.MaxValue);
            Assert.Equal(myDouble, double.MaxValue);
            Assert.Equal(myDecimal, decimal.MaxValue);
            Assert.Equal(myBoolean, true);
            Assert.Equal(myDateTime, default(DateTime?)); //null
            Assert.Equal(myDateTimeOffset, default(DateTimeOffset?)); //null
            Assert.Equal(myTimeSpan, default(TimeSpan?)); //null
            Assert.Equal(myChar, 'c');
            return 0;
        }
    }
}