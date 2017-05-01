using NetStitch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharedProjectValueTask
{
    public interface IValueTask : INetStitchContract
    {
        ValueTask<int> TallyAsync(int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }
}
