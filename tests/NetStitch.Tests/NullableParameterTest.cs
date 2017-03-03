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
    public class NullableParameterTest
    {
        [Fact]
        public async Task NullableParameter()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            using (var server = new TestServer(host))
            {
                var client = server.CreateClient();
                var stub = new NetStitchClient("http://localhost/", client).Create<client.INullableParameter>();

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
    public class NullableParameters : server.INullableParameter
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
    namespace client
    {
        [NetStitchContract]
        public interface INullableParameter
        {
            [Operation("NullableParameterTest1")]
            Task<int> NullableParameterTest1Async(
                String myString,
                SByte? mySByte,
                Byte? myByte,
                Int16? myInt16,
                Int32? myInt32,
                Int64? myInt64,
                UInt16? myUInt16,
                UInt32? myUInt32,
                UInt64? myUInt64,
                Single? mySingle,
                Double? myDouble,
                Decimal? myDecimal,
                Boolean? myBoolean,
                DateTime? myDateTime,
                DateTimeOffset? myDateTimeOffset,
                TimeSpan? myTimeSpan,
                Char? myChar
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
            [Operation("NullableParameterTest2")]
            Task<int> NullableParameterTest2Async(
                String myString,
                SByte? mySByte,
                Byte? myByte,
                Int16? myInt16,
                Int32? myInt32,
                Int64? myInt64,
                UInt16? myUInt16,
                UInt32? myUInt32,
                UInt64? myUInt64,
                Single? mySingle,
                Double? myDouble,
                Decimal? myDecimal,
                Boolean? myBoolean,
                DateTime? myDateTime,
                DateTimeOffset? myDateTimeOffset,
                TimeSpan? myTimeSpan,
                Char? myChar
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );

            [Operation("NullableParameterTest3")]
            Task<int> NullableParameterTest3Async(
                String myString = "myString",
                SByte? mySByte = SByte.MaxValue,
                Byte? myByte = Byte.MaxValue,
                Int16? myInt16 = Int16.MaxValue,
                Int32? myInt32 = Int32.MaxValue,
                Int64? myInt64 = Int64.MaxValue,
                UInt16? myUInt16 = UInt16.MaxValue,
                UInt32? myUInt32 = UInt32.MaxValue,
                UInt64? myUInt64 = UInt64.MaxValue,
                Single? mySingle = Single.MaxValue,
                Double? myDouble = Double.MaxValue,
                Decimal? myDecimal = Decimal.MaxValue,
                Boolean? myBoolean = true,
                DateTime? myDateTime = default(DateTime?),
                DateTimeOffset? myDateTimeOffset = default(DateTimeOffset?),
                TimeSpan? myTimeSpan = default(TimeSpan?),
                Char? myChar = 'c'
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );

        }
    }
    namespace server
    {
        [NetStitchContract]
        public interface INullableParameter
        {
            [Operation("NullableParameterTest1")]
            int NullableParameterTest1(
                String myString,
                SByte? mySByte,
                Byte? myByte,
                Int16? myInt16,
                Int32? myInt32,
                Int64? myInt64,
                UInt16? myUInt16,
                UInt32? myUInt32,
                UInt64? myUInt64,
                Single? mySingle,
                Double? myDouble,
                Decimal? myDecimal,
                Boolean? myBoolean,
                DateTime? myDateTime,
                DateTimeOffset? myDateTimeOffset,
                TimeSpan? myTimeSpan,
                Char? myChar
                );
            [Operation("NullableParameterTest2")]
            int NullableParameterTest2(
                String myString,
                SByte? mySByte,
                Byte? myByte,
                Int16? myInt16,
                Int32? myInt32,
                Int64? myInt64,
                UInt16? myUInt16,
                UInt32? myUInt32,
                UInt64? myUInt64,
                Single? mySingle,
                Double? myDouble,
                Decimal? myDecimal,
                Boolean? myBoolean,
                DateTime? myDateTime,
                DateTimeOffset? myDateTimeOffset,
                TimeSpan? myTimeSpan,
                Char? myChar
                );
            [Operation("NullableParameterTest3")]
            int NullableParameterTest3(
                String myString = "myString",
                SByte? mySByte = SByte.MaxValue,
                Byte? myByte = Byte.MaxValue,
                Int16? myInt16 = Int16.MaxValue,
                Int32? myInt32 = Int32.MaxValue,
                Int64? myInt64 = Int64.MaxValue,
                UInt16? myUInt16 = UInt16.MaxValue,
                UInt32? myUInt32 = UInt32.MaxValue,
                UInt64? myUInt64 = UInt64.MaxValue,
                Single? mySingle = Single.MaxValue,
                Double? myDouble = Double.MaxValue,
                Decimal? myDecimal = Decimal.MaxValue,
                Boolean? myBoolean = true,
                DateTime? myDateTime = default(DateTime?),
                DateTimeOffset? myDateTimeOffset = default(DateTimeOffset?),
                TimeSpan? myTimeSpan = default(TimeSpan?),
                Char? myChar = 'c'
                );
        }
    }
}
