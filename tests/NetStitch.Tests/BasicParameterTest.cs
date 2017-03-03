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
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
            app.UseNetStitch(typeof(Startup));
        }
    }
    public class BasicParameterTest
    {
        [Fact]
        public async Task BasicParameter()
        {
            var config = new ConfigurationBuilder().Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseStartup<Startup>();

            using (var server = new TestServer(host))
            {
                var client = server.CreateClient();
                var stub = new NetStitchClient("http://localhost/", client).Create<client.IBasicParameter>();
                var result = await stub.BasicParameterAsync(
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
                    'c',
                    new string[] { "abc", "edf" },
                    new sbyte[] { sbyte.MinValue, sbyte.MaxValue },
                    new byte[] { byte.MinValue, byte.MaxValue },
                    new short[] { short.MinValue, short.MinValue },
                    new int[] { int.MinValue, int.MaxValue },
                    new long[] { long.MinValue, long.MaxValue },
                    new ushort[] { ushort.MinValue, ushort.MaxValue },
                    new uint[] { uint.MinValue, uint.MaxValue },
                    new ulong[] { ulong.MinValue, ulong.MaxValue },
                    new float[] { float.MinValue, float.MaxValue },
                    new double[] { double.MinValue, double.MaxValue },
                    new decimal[] { decimal.MinValue, decimal.MaxValue },
                    new bool[] { true, false },
                    new DateTime[] { new DateTime(2017, 1, 1).ToUniversalTime(), new DateTime(2017, 1, 2).ToUniversalTime() },
                    new DateTimeOffset[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue },
                    new TimeSpan[] { new TimeSpan(1, 1, 1, 1, 1), new TimeSpan(2, 2, 2, 2, 2) },
                    "char".ToCharArray()
                    );
                await stub.BasicParameterDefaultAsync();
            }
        }
    }
    public class BasicParameters : server.IBasicParameter
    {
        public int BasicParameter(string myString, sbyte mySByte, byte myByte, short myInt16, int myInt32, long myInt64, ushort myUInt16, uint myUInt32, ulong myUInt64, float mySingle, double myDouble, decimal myDecimal, bool myBoolean, DateTime myDateTime, DateTimeOffset myDateTimeOffset, TimeSpan myTimeSpan, char myChar, string[] myStringArray, sbyte[] mySByteArray, byte[] myByteArray, short[] myInt16Array, int[] myInt32Array, long[] myInt64Array, ushort[] myUInt16Array, uint[] myUInt32Array, ulong[] myUInt64Array, float[] mySingleArray, double[] myDoubleArray, decimal[] myDecimalArray, bool[] myBooleanArray, DateTime[] myDateTimeArray, DateTimeOffset[] myDateTimeOffsetArray, TimeSpan[] myTimeSpanArray, char[] myCharArray)
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
            Assert.Equal(myStringArray, new string[] { "abc", "edf" });
            Assert.Equal(mySByteArray, new sbyte[] { sbyte.MinValue, sbyte.MaxValue });
            Assert.Equal(myByteArray, new byte[] { byte.MinValue, byte.MaxValue });
            Assert.Equal(myInt16Array, new short[] { short.MinValue, short.MinValue });
            Assert.Equal(myInt32Array, new int[] { int.MinValue, int.MaxValue });
            Assert.Equal(myInt64Array, new long[] { long.MinValue, long.MaxValue });
            Assert.Equal(myUInt16Array, new ushort[] { ushort.MinValue, ushort.MaxValue });
            Assert.Equal(myUInt32Array, new uint[] { uint.MinValue, uint.MaxValue });
            Assert.Equal(myUInt64Array, new ulong[] { ulong.MinValue, ulong.MaxValue });
            Assert.Equal(mySingleArray, new float[] { float.MinValue, float.MaxValue });
            Assert.Equal(myDoubleArray, new double[] { double.MinValue, double.MaxValue });
            Assert.Equal(myDecimalArray, new decimal[] { decimal.MinValue, decimal.MaxValue });
            Assert.Equal(myBooleanArray, new bool[] { true, false });
            Assert.Equal(myDateTimeArray, new DateTime[] { new DateTime(2017, 1, 1).ToUniversalTime(), new DateTime(2017, 1, 2).ToUniversalTime() });
            Assert.Equal(myDateTimeOffsetArray, new DateTimeOffset[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue });
            Assert.Equal(myTimeSpanArray, new TimeSpan[] { new TimeSpan(1, 1, 1, 1, 1), new TimeSpan(2, 2, 2, 2, 2) });
            Assert.Equal(myCharArray, "char".ToCharArray());
            return 1;
        }

        public int BasicParameterDefault(string myString = "myString", sbyte mySByte = sbyte.MaxValue, byte myByte = 255, short myInt16 = short.MaxValue, int myInt32 = int.MaxValue, long myInt64 = long.MaxValue, ushort myUInt16 = ushort.MaxValue, uint myUInt32 = uint.MaxValue, ulong myUInt64 = ulong.MaxValue, float mySingle = float.MaxValue, double myDouble = double.MaxValue, decimal myDecimal = decimal.MaxValue, bool myBoolean = true, DateTime myDateTime = default(DateTime), DateTimeOffset myDateTimeOffset = default(DateTimeOffset), TimeSpan myTimeSpan = default(TimeSpan), char myChar = 'c')
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
            Assert.Equal(myDateTime, default(DateTime));
            Assert.Equal(myDateTimeOffset, default(DateTimeOffset));
            Assert.Equal(myTimeSpan, default(TimeSpan));
            Assert.Equal(myChar, 'c');
            return 0;
        }
    }
    namespace client
    {
        [NetStitchContract]
        public interface IBasicParameter
        {
            [Operation("BasicParameter1")]
            Task<int> BasicParameterAsync(
                String myString,
                SByte mySByte,
                Byte myByte,
                Int16 myInt16,
                Int32 myInt32,
                Int64 myInt64,
                UInt16 myUInt16,
                UInt32 myUInt32,
                UInt64 myUInt64,
                Single mySingle,
                Double myDouble,
                Decimal myDecimal,
                Boolean myBoolean,
                DateTime myDateTime,
                DateTimeOffset myDateTimeOffset,
                TimeSpan myTimeSpan,
                Char myChar,
                String[] myStringArray,
                SByte[] mySByteArray,
                Byte[] myByteArray,
                Int16[] myInt16Array,
                Int32[] myInt32Array,
                Int64[] myInt64Array,
                UInt16[] myUInt16Array,
                UInt32[] myUInt32Array,
                UInt64[] myUInt64Array,
                Single[] mySingleArray,
                Double[] myDoubleArray,
                Decimal[] myDecimalArray,
                Boolean[] myBooleanArray,
                DateTime[] myDateTimeArray,
                DateTimeOffset[] myDateTimeOffsetArray,
                TimeSpan[] myTimeSpanArray,
                Char[] myCharArray
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
            [Operation("BasicParameter2")]
            Task<int> BasicParameterDefaultAsync(
                String myString = "myString",
                SByte mySByte = SByte.MaxValue,
                Byte myByte = Byte.MaxValue,
                Int16 myInt16 = Int16.MaxValue,
                Int32 myInt32 = Int32.MaxValue,
                Int64 myInt64 = Int64.MaxValue,
                UInt16 myUInt16 = UInt16.MaxValue,
                UInt32 myUInt32 = UInt32.MaxValue,
                UInt64 myUInt64 = UInt64.MaxValue,
                Single mySingle = Single.MaxValue,
                Double myDouble = Double.MaxValue,
                Decimal myDecimal = Decimal.MaxValue,
                Boolean myBoolean = true,
                DateTime myDateTime = default(DateTime),
                DateTimeOffset myDateTimeOffset = default(DateTimeOffset),
                TimeSpan myTimeSpan = default(TimeSpan),
                Char myChar = 'c'
                , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
            );
        }
    }
    namespace server
    {
        [NetStitchContract]
        public interface IBasicParameter
        {
            [Operation("BasicParameter1")]
            int BasicParameter(
                String myString,
                SByte mySByte,
                Byte myByte,
                Int16 myInt16,
                Int32 myInt32,
                Int64 myInt64,
                UInt16 myUInt16,
                UInt32 myUInt32,
                UInt64 myUInt64,
                Single mySingle,
                Double myDouble,
                Decimal myDecimal,
                Boolean myBoolean,
                DateTime myDateTime,
                DateTimeOffset myDateTimeOffset,
                TimeSpan myTimeSpan,
                Char myChar,
                String[] myStringArray,
                SByte[] mySByteArray,
                Byte[] myByteArray,
                Int16[] myInt16Array,
                Int32[] myInt32Array,
                Int64[] myInt64Array,
                UInt16[] myUInt16Array,
                UInt32[] myUInt32Array,
                UInt64[] myUInt64Array,
                Single[] mySingleArray,
                Double[] myDoubleArray,
                Decimal[] myDecimalArray,
                Boolean[] myBooleanArray,
                DateTime[] myDateTimeArray,
                DateTimeOffset[] myDateTimeOffsetArray,
                TimeSpan[] myTimeSpanArray,
                Char[] myCharArray
                );
            [Operation("BasicParameter2")]
            int BasicParameterDefault(
                String myString = "myString",
                SByte mySByte = SByte.MaxValue,
                Byte myByte = Byte.MaxValue,
                Int16 myInt16 = Int16.MaxValue,
                Int32 myInt32 = Int32.MaxValue,
                Int64 myInt64 = Int64.MaxValue,
                UInt16 myUInt16 = UInt16.MaxValue,
                UInt32 myUInt32 = UInt32.MaxValue,
                UInt64 myUInt64 = UInt64.MaxValue,
                Single mySingle = Single.MaxValue,
                Double myDouble = Double.MaxValue,
                Decimal myDecimal = Decimal.MaxValue,
                Boolean myBoolean = true,
                DateTime myDateTime = default(DateTime),
                DateTimeOffset myDateTimeOffset = default(DateTimeOffset),
                TimeSpan myTimeSpan = default(TimeSpan),
                Char myChar = 'c'
            );
        }
    }
}
