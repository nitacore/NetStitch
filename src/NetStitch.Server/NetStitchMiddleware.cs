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
using System.Collections.Concurrent;

namespace NetStitch.Server
{
    public class NetStitchMiddleware
    {
        static NetStitchMiddleware()
        {
            Servers = new ConcurrentDictionary<string, NetStitchServer>();
        }

        public static readonly ConcurrentDictionary<string, NetStitchServer> Servers;

        readonly NetStitchServer server;

        public NetStitchMiddleware(RequestDelegate next, Type type)
             : this(next, type, new NetStitchOption()) { }

        public NetStitchMiddleware(RequestDelegate next, Type type, NetStitchOption option)
            : this(next, new[] { type.GetTypeInfo().Assembly }, option) { }

        public NetStitchMiddleware(RequestDelegate next, Assembly[] assemblies, NetStitchOption option)
        {
            server = new NetStitchServer(assemblies, option);
            if (!Servers.TryAdd(option.ServerID, server))
            {
                throw new InvalidOperationException($"Duplicate Server ID : {option.ServerID}");
            }
        }
        public Task Invoke(HttpContext httpContext)
        {
            return server.OperationExecuteAsync(httpContext);
        }
    }
}

