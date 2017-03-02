using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetStitch
{
    public interface IOperationContext
    {
        OperationContext Context { set; }
    }
}
