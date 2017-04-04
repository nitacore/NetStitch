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
        [NetStitch.Operation("dfe985ad-aa84-4497-b324-608ea025d5da")]
#if !___server___
        Task<
#endif
        string
#if !___server___
        >
#endif
#if !___server___
        HttpContextTestAsync
#else
        HttpContextTest
#endif
        (
            string myString

#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
                    );
    }
}
