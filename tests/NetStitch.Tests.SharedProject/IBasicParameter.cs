﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if !___server___
namespace NetStitch.Tests.Client
#else
namespace NetStitch.Tests.Server
#endif
{
    public interface IBasicParameterTest : INetStitchContract
    {
        ValueTask<int> BasicParameterAsync
        (
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

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );

        ValueTask<int> BasicParameterDefaultAsync(
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
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                );
    }
}
