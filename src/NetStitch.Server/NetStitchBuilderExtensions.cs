using NetStitch.Option;
using NetStitch.Server;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Builder
{
    public static class NetStitchBuilderExtensions
    {
        public static IApplicationBuilder UseNetStitch(this IApplicationBuilder builder, Type type)
        {
            return builder.UseMiddleware<NetStitchMiddleware>(type);
        }
        public static IApplicationBuilder UseNetStitch(this IApplicationBuilder builder, Type type, NetStitchOption option)
        {
            return builder.UseMiddleware<NetStitchMiddleware>(type, option);
        }
        public static IApplicationBuilder UseNetStitch(this IApplicationBuilder builder, Assembly[] assemblies, NetStitchOption option)
        {
            return builder.UseMiddleware<NetStitchMiddleware>(assemblies, option);
        }
    }
}
