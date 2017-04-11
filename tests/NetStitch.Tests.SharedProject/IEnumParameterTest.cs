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
    public interface IEnumParameterTest : INetStitchContract
    {
        ValueTask<MyEnum> EnumParameterTestAsync
        ( MyEnum myEnum

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
        ValueTask<MyEnumFlag> EnumFlagParameterTestAsync
        ( MyEnumFlag myEnum
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
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
