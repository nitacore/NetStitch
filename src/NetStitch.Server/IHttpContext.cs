using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetStitch.Server
{
    public interface IHttpContext
    {
        HttpContext Context { set; }
    }
}
