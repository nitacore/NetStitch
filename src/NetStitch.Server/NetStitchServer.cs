﻿using Microsoft.AspNetCore.Http;
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
        private readonly IDictionary<string, OperationController> OperationDic = new Dictionary<string, OperationController>();

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
            //.Select(x => x.classType.GetInterfaceMap(x.interfaceType)) 
            .SelectMany(
            //x => x.TargetMethods.Zip(x.InterfaceMethods, (targetMethod, interfaceMethod) => new { targetMethod, interfaceMethod }),
             x => x.interfaceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
            (x, methodInfo) =>
            {
                var targetMethod = x.targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Single(m => IsSameNamesapceAndMethodName(m, methodInfo) &&
                m.CallingConvention == methodInfo.CallingConvention &&
                m.ReturnType == methodInfo.ReturnType &&
                m.GetGenericArguments().SequenceEqual(methodInfo.GetGenericArguments()) &&
                m.GetParameters().Select(p => p.ParameterType).SequenceEqual(methodInfo.GetParameters().Select(p => p.ParameterType)));
                return new
                {
                    x.targetType,
                    x.interfaceType,
                    targetMethod,
                    interfaceMethod = methodInfo,
                };
            });

            foreach (var element in seq)
            {
                var op = new OperationController(element.targetType, element.interfaceType, element.targetMethod, element.interfaceMethod, option);
                OperationDic.Add(op.OperationID, op);
            }

            this.option.Logger.ServerSetupCompleted(tm.Elapsed);

        }

        private bool IsSameNamesapceAndMethodName(MethodInfo targetMethod, MethodInfo interfaceMethod)
        {
            string name;
            var targetMethodSplit = targetMethod.Name.Split('.');
            if (targetMethodSplit.Length > 1)
            {
                name = $"{interfaceMethod.DeclaringType.ToString()}.{interfaceMethod.Name}";
            }
            else
            {
                name = interfaceMethod.Name;
            }
            return targetMethod.Name == name;
        }

        public async Task OperationExecuteAsync(HttpContext httpContext)
        {

            OperationController @operation;
            if (!OperationDic.TryGetValue(httpContext.Request.Path.Value.TrimStart('/'), out @operation))
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