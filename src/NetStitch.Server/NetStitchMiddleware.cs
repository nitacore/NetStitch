using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Reflection.Emit;
using NetStitch.Option;
using NetStitch.Logger;

namespace NetStitch.Server
{
    public class NetStitchMiddleware
    {
        static NetStitchMiddleware()
        {
        }

        readonly NetStitchServer server;

        public NetStitchMiddleware(RequestDelegate next, Type type)
             : this(next, type, new NetStitchOption()) { }

        public NetStitchMiddleware(RequestDelegate next, Type type, NetStitchOption option)
            : this(next, new[] { type.GetTypeInfo().Assembly }, option) { }

        public NetStitchMiddleware(RequestDelegate next, Assembly[] assemblies, NetStitchOption option)
        {
            option.Logger = option.Logger ?? new NetStitchEmptyLogger();
            option.GlobalFilters = option.GlobalFilters ?? new NetStitchFilterAttribute[0];
            server = new NetStitchServer(assemblies, option);
        }
        public async Task Invoke(HttpContext httpContext)
        {
            await server.OperationExecuteAsync(httpContext).ConfigureAwait(false);
        }
    }
}

