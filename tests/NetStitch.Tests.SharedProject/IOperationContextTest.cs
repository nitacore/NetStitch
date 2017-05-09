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
    public interface IOperationContextTest : INetStitchContract
    {
        [NetStitch.Operation]
        ValueTask<string> HttpContextTestAsync
        (string myString
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
}
