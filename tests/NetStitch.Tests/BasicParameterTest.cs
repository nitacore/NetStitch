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
    using NetStitch.Tests.Client;

    [Collection(nameof(ServerCollection))]
    public class BasicParameterTest
    {
        ServerFixture server;
        public BasicParameterTest(ServerFixture server)
        {
            this.server = server;
        }

        [Fact]
        public async Task BasicParameter()
        {
            var stub = server.CreateStub<IBasicParameterTest>();
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
namespace NetStitch.Tests
{
    using NetStitch.Tests.Server;
    public class BasicParameterServer : IBasicParameterTest
    {
        public async ValueTask<int> BasicParameterAsync(string myString, sbyte mySByte, byte myByte, short myInt16, int myInt32, long myInt64, ushort myUInt16, uint myUInt32, ulong myUInt64, float mySingle, double myDouble, decimal myDecimal, bool myBoolean, DateTime myDateTime, DateTimeOffset myDateTimeOffset, TimeSpan myTimeSpan, char myChar, string[] myStringArray, sbyte[] mySByteArray, byte[] myByteArray, short[] myInt16Array, int[] myInt32Array, long[] myInt64Array, ushort[] myUInt16Array, uint[] myUInt32Array, ulong[] myUInt64Array, float[] mySingleArray, double[] myDoubleArray, decimal[] myDecimalArray, bool[] myBooleanArray, DateTime[] myDateTimeArray, DateTimeOffset[] myDateTimeOffsetArray, TimeSpan[] myTimeSpanArray, char[] myCharArray)
        {
            Assert.Equal("myString", myString);
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
            Assert.True(myBoolean);
            Assert.Equal(myDateTime, new DateTime(2000, 1, 1).ToUniversalTime());
            Assert.Equal(myDateTimeOffset, DateTimeOffset.MaxValue);
            Assert.Equal(myTimeSpan, new TimeSpan(1, 1, 1, 1, 1));
            Assert.Equal('c', myChar);
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

        public async ValueTask<int> BasicParameterDefaultAsync(string myString = "myString", sbyte mySByte = sbyte.MaxValue, byte myByte = 255, short myInt16 = short.MaxValue, int myInt32 = int.MaxValue, long myInt64 = long.MaxValue, ushort myUInt16 = ushort.MaxValue, uint myUInt32 = uint.MaxValue, ulong myUInt64 = ulong.MaxValue, float mySingle = float.MaxValue, double myDouble = double.MaxValue, decimal myDecimal = decimal.MaxValue, bool myBoolean = true, DateTime myDateTime = default(DateTime), DateTimeOffset myDateTimeOffset = default(DateTimeOffset), TimeSpan myTimeSpan = default(TimeSpan), char myChar = 'c')
        {
            Assert.Equal("myString", myString);
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
            Assert.True(myBoolean);
            Assert.Equal(default(DateTime), myDateTime);
            Assert.Equal(default(DateTimeOffset), myDateTimeOffset);
            Assert.Equal(default(TimeSpan), myTimeSpan);
            Assert.Equal('c', myChar);
            return 0;
        }
    }
}