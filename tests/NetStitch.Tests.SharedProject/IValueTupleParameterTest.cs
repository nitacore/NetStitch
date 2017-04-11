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
        ValueTask<(int a, (int b, int c) d)> ValueTupleParameterTestAsync( (int a, (int b, int c) d) valuetuple
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
        ValueTask<(int a, int b)> ValueTupleParameterTest2Async( (int a, int b) valuetuple
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
}
