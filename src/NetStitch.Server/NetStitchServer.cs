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
        public readonly IDictionary<string, OperationController> OperationDic = new Dictionary<string, OperationController>();

        public readonly NetStitchOption option;

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
            .Where(x => x.GetInterfaces().Any(t => t == typeof(INetStitchContract)))
            .Select(x =>
            {
                try
                {
                    return new
                    {
                        interfaceType = x,
                        targetType = types.Where(t => t.GetInterfaces().Any(t2 => t2 == x)).SingleOrDefault()
                    };
                }
                catch (InvalidOperationException)
                {
                    throw;
                }

            })
            .Where(x => x.targetType != null)
            .Where(x => x.targetType.GetTypeInfo().IsAbstract == false)
            .Select(x => x.targetType.GetTypeInfo().GetRuntimeInterfaceMap(x.interfaceType)) 
            .SelectMany(
            x => x.TargetMethods.Zip(x.InterfaceMethods, (targetMethod, interfaceMethod) => new { targetMethod, interfaceMethod }),
            (x, methods) =>
            {
                return new
                {
                    x.TargetType,
                    x.InterfaceType,
                    methods.targetMethod,
                    methods.interfaceMethod,
                };
            });

            foreach (var element in seq)
            {
                var op = new OperationController(element.TargetType, element.InterfaceType, element.targetMethod, element.interfaceMethod, option);
                OperationDic.Add(op.OperationID, op);
            }

            this.option.Logger.ServerSetupCompleted(tm.Elapsed);

        }

        public async Task OperationExecuteAsync(HttpContext httpContext)
        {

            OperationController @operation;
            if (!OperationDic.TryGetValue(httpContext.Request.Path.Value, out @operation))
            {
                //Operation Not Found
                httpContext.Response.StatusCode = HttpStatus.NotFound;
                return;
            }

            option.Logger.OperationStart(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name);

            try
            {

                var sw = System.Diagnostics.Stopwatch.StartNew();

                var operationContext = new OperationContext(httpContext, @operation, option);

                await @operation.OperationAsync(operationContext).ConfigureAwait(false);
                
                option.Logger.OperationCompleted(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name, sw.Elapsed);

            }
            catch (Exception ex)
            {
                option.Logger.OperationError(@operation.InterfaceType.Name, @operation.ClassType.Name, @operation.MethodInfo.Name, ex);

                switch (option.ExceptionHandling)
                {
                    case ExceptionHandling.HideMessage:
                        httpContext.Response.StatusCode = HttpStatus.InternalServerError;
                        break;
                    case ExceptionHandling.ShowMessage:
                    default:
                        httpContext.Response.StatusCode = HttpStatus.InternalServerError;
                        httpContext.Request.ContentType = "text/plain";
                        var bytes = new UTF8Encoding(false).GetBytes(ex.Message);
                        httpContext.Response.Body.Write(bytes, 0, bytes.Length);
                        break;
                }
            }
        }
    }
}