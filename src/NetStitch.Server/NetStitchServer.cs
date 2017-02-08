using Microsoft.AspNetCore.Http;
using NetStitch.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NetStitch.Server
{
    public class NetStitchServer
    {
        public readonly IDictionary<string, OperationController> dic = new Dictionary<string, OperationController>();

        private readonly NetStitchOption option;

        public NetStitchServer(Assembly[] assemblies, NetStitchOption option)
        {
            this.option = option;
            this.option.Logger.ServerSetupStart();

            var tm = System.Diagnostics.Stopwatch.StartNew();

            var types = assemblies
            .SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null);
                }
            });

            var seq = types
            .Where(x => x.GetTypeInfo().GetCustomAttribute<NetStitchContractAttribute>() != null)
            .Select(x =>
            {
                try
                {
                    return new
                    {
                        interfaceType = x,
                        classType = types.Where(t => t.GetInterfaces().Any(t2 => t2 == x)).SingleOrDefault()
                    };
                }
                catch (InvalidOperationException)
                {
                    throw;
                }

            })
            .Where(x => x.classType != null)
            .Where(x => x.classType.GetTypeInfo().IsAbstract == false)
            .SelectMany(
             x => x.interfaceType.GetMethods(),
            (x, methodInfo) =>
            {
                return new
                {
                    x.classType,
                    x.interfaceType,
                    methodInfo,
                };
            });

            foreach (var element in seq)
            {
                var op = new OperationController(element.classType, element.interfaceType, element.methodInfo);
                dic.Add(op.OperationID, op);
            }

            this.option.Logger.ServerSetupCompleted(tm.Elapsed);

        }

        public async Task OperationExecuteAsync(HttpContext httpContext)
        {

            if (httpContext.Request.Method.ToUpper() != "POST")
                return;

            OperationController @operation;
            if (!dic.TryGetValue(httpContext.Request.Path.Value.TrimStart('/'), out @operation))
            {
                //Operation Not Found
                httpContext.Response.StatusCode = HttpStatus.NotFound;
                return;
            }

            option.Logger.OperationStart(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name);

            try
            {

                var sw = System.Diagnostics.Stopwatch.StartNew();

                await @operation.ExecuteAsync(httpContext).ConfigureAwait(false);

                option.Logger.OperationCompleted(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name, sw.Elapsed);

            }
            catch (Exception ex)
            {
                option.Logger.OperationError(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name, ex);

                httpContext.Response.StatusCode = HttpStatus.InternalServerError;
                httpContext.Request.ContentType = "text/plain";
                var bytes = new UTF8Encoding(false).GetBytes(ex.Message);
                httpContext.Response.Body.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
