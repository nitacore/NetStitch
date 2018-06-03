using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetStitch;

namespace SharedProjectValueTuple
{
    public interface ISharedInterfaceValueTuple : INetStitchContract
    {
        ValueTask<(int sum, int count)> TallyAsync(IList<(int a, int b)> tes
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
        ValueTask<(int sum, int count)> Tally(IList<(int a, int b)> tes);
    }
}
