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
        [NetStitch.Operation("a0c323a5-eb97-4087-a0b8-d0afd8773b4a")]
#if !___server___
        Task<
#endif
        MyEnum
#if !___server___
        >
#endif
#if !___server___
        EnumParameterTestAsync
#else
        EnumParameterTest
#endif
        (
            MyEnum myEnum

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
        [NetStitch.Operation("50853428-242a-4f27-b531-a01d724f3d71")]
#if !___server___
        Task<
#endif
        MyEnumFlag
#if !___server___
        >
#endif
#if !___server___
        EnumFlagParameterTestAsync
#else
        EnumFlagParameterTest
#endif
        (
            MyEnumFlag myEnum

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
