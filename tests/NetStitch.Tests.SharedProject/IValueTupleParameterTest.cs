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
    public interface IValueTupleParameterTest : INetStitchContract
    {
        [NetStitch.Operation("98aeb629-0f9f-4962-958f-7b85326ba571")]
#if !___server___
        Task<
#endif
        (int a, (int b, int c) d)
#if !___server___
        >
#endif
#if !___server___
        ValueTupleParameterTestAsync
#else
        ValueTupleParameterTest
#endif
        (
            (int a, (int b, int c) d) valuetuple

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
        [NetStitch.Operation("3c9fd52d-24f3-484b-9d09-f72f19fc6ed8")]
#if !___server___
        Task<
#endif
        (int a, int b)
#if !___server___
        >
#endif
#if !___server___
        ValueTupleParameterTest2Async
#else
        ValueTupleParameterTest2
#endif
        (
            (int a, int b) valuetuple

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
    }
}
