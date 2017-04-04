using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if !___server___
namespace NetStitch.Tests.Client
#else
namespace NetStitch.Tests.Server
#endif
{
    public interface INullableParameterTest : INetStitchContract
    {
        [NetStitch.Operation("d4b50183-240f-4b02-bb15-417ca5ccb74e")]
#if !___server___
        Task<
#endif
        int
#if !___server___
        >
#endif
#if !___server___
        NullableParameterTest1Async
#else
        NullableParameterTest1
#endif
        (
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

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
        [NetStitch.Operation("0fd8c7f1-94b3-43e3-9d10-88160fcd17c3")]
#if !___server___
        Task<
#endif
        int
#if !___server___
        >
#endif
#if !___server___
        NullableParameterTest2Async
#else
        NullableParameterTest2
#endif
        (
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

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
        [NetStitch.Operation("1e88e4ba-74c0-4f33-9d7b-20fff3072ee2")]
#if !___server___
        Task<
#endif
        int
#if !___server___
        >
#endif
#if !___server___
        NullableParameterTest3Async
#else
        NullableParameterTest3
#endif
        (
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

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
    }
}
