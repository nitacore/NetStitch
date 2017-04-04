using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetStitch;

namespace SharedProjectValueTuple
{
    public interface ISharedInterfaceValueTuple : INetStitchContract
    {
        [NetStitch.Operation("0fe342a1-56eb-4c2c-ae88-a211b130e680")]
#if !___server___
        Task<
#endif
        (int sum, int count)
#if !___server___
        >
#endif
#if !___server___
        TallyAsync
#else
        Tally
#endif
        (IList<(int a, int b)> tes
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
}
