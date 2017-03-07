using MessagePack;
using Microsoft.AspNetCore.Http;
using NetStitch.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NetStitch.Server
{
    public class OperationController
    {
        private enum OperationType
        {
            Action = 0,
            Operation = 1,
            AsyncOperation = 2,
        }

        public readonly string OperationID;
        public readonly Type ClassType;
        public readonly Type InterfaceType;
        public readonly MethodInfo MethodInfo;
        public readonly Type ParameterStructType;
        
        private readonly Action<OperationContext> action;
        private readonly Func<OperationContext, byte[]> function;
        internal readonly Func<OperationContext, Task> OperationAsync;
        internal readonly NetStitchFilterAttribute[] filters;

        //private readonly InnerMiddlewareAttribute[] innerMiddlewares;

        private Type CreateParameterSturctType(Type interfaceType, MethodInfo methodInfo)
        {
            var assemblyName = new AssemblyName($"___{interfaceType.FullName}.{methodInfo.Name}");

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var typeBuilder = moduleBuilder.DefineType($"___{interfaceType.FullName}.{methodInfo.Name}",
                TypeAttributes.Public |
                TypeAttributes.SequentialLayout |
                TypeAttributes.AnsiClass |
                TypeAttributes.Sealed |
                TypeAttributes.BeforeFieldInit,
                typeof(ValueType));

            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                methodInfo.GetParameters().Select(x => x.ParameterType).ToArray()
                );

            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(MessagePackObjectAttribute).GetTypeInfo().GetConstructor(new Type[] { typeof(bool) }), new object[] { false }));

            var il = ctor.GetILGenerator();

            var seq = methodInfo.GetParameters().Select((x, index) => new { name = x.Name, parameterType = x.ParameterType, index });

            foreach (var item in seq)
            {
                var field = typeBuilder.DefineField(item.name, item.parameterType, FieldAttributes.Public);
                field.SetCustomAttribute(new CustomAttributeBuilder(typeof(KeyAttribute).GetConstructor(new[] { typeof(int) }), new object[] { item.index }));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, item.index + 1);
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo().AsType();

        }

        public OperationController(Type targetType, Type interfaceType, MethodInfo targetMethodInfo, MethodInfo interfaceMethodInfo, NetStitchOption option)
        {

            this.ClassType = targetType;
            this.InterfaceType = interfaceType;
            this.MethodInfo = targetMethodInfo;
            this.ParameterStructType = CreateParameterSturctType(interfaceType, targetMethodInfo);
            this.OperationID = ((OperationAttribute)interfaceMethodInfo.GetCustomAttribute(typeof(OperationAttribute))).OperationID;

            bool requiresOperationContext = targetType.GetInterfaces().Any(x => x == typeof(IOperationContext));

            bool operationIsAsyncType = typeof(Task).IsAssignableFrom(targetMethodInfo.ReturnType);

            bool operationIsAsyncFunction = operationIsAsyncType && targetMethodInfo.ReturnType.GenericTypeArguments.Length != 0;

            Type asyncRetunType = targetMethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault();

            MethodInfo deserializeMethod = typeof(MessagePackSerializer).GetMethods()
                                           .First(x => x.Name == (nameof(MessagePackSerializer.Deserialize)) &&
                                                       x.IsGenericMethod &&
                                                       x.GetParameters().Any(p => p.ParameterType == typeof(System.IO.Stream) && 
                                                       x.GetParameters().Length == 1))
                                           .MakeGenericMethod(new Type[] { ParameterStructType });
            // Context
            var operationContext = Expression.Parameter(typeof(OperationContext), nameof(OperationContext));
            var bindContext = Expression.Bind(typeof(IOperationContext).GetProperty(nameof(IOperationContext.Context)), operationContext);

            // new Class() or new Class() { Context = Context }
            var newClass = requiresOperationContext ?
            Expression.MemberInit(Expression.New(targetType), bindContext) :
            Expression.MemberInit(Expression.New(targetType));

            // Context.HttpContext.Request.Body
            var httpContext = Expression.Property(operationContext, nameof(HttpContext));
            var request = Expression.Property(httpContext, nameof(HttpContext.Request));
            var body = Expression.Property(request, nameof(HttpRequest.Body));

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            var deserialize = Expression.Call(null, deserializeMethod, body);
            var obj = Expression.Parameter(ParameterStructType, "obj");
            var assign = Expression.Assign(obj, deserialize);

            // obj.field1, obj.field2, ...
            var args = targetMethodInfo.GetParameters().Select(x => Expression.Field(obj, x.Name)).ToArray();

            // new Class().Method(obj.field1, obj.field2, ...)
            var callMethod = Expression.Call(newClass, targetMethodInfo, args);

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            // new Class().Method(obj.field1, obj.field2, ...)
            var block = Expression.Block(new[] { obj }, assign, callMethod);

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            // AsyncExecute(new Class().Method(obj.field1, obj.field2, ...))
            if (operationIsAsyncType)
            {
                var asyncExecuteMethodInfo = operationIsAsyncFunction ?
                    typeof(OperationController).GetMethod(nameof(OperationController.AsyncFunction)).MakeGenericMethod(asyncRetunType) :
                    typeof(OperationController).GetMethod(nameof(OperationController.AsyncAction));

                var taskExecute = Expression.Call(null, asyncExecuteMethodInfo, operationContext, block);
                var lambda = Expression.Lambda<Func<OperationContext, Task>>(taskExecute, operationContext);
                OperationAsync = lambda.Compile();
            }
            else
            {
                // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                // new Class().Method(obj.field1, obj.field2, ...)
                if (targetMethodInfo.ReturnType == typeof(void))
                {
                    var lambda = Expression.Lambda<Action<OperationContext>>(block, operationContext);
                    this.action = lambda.Compile();
                    this.OperationAsync = (Func<OperationContext, Task>)this.GetType().GetMethod("Action").CreateDelegate(typeof(Func<OperationContext, Task>), this);
                }
                else
                {

                    MethodInfo serializeMethod = typeof(MessagePackSerializer).GetMethods()
                                                 .First(x => x.Name == (nameof(MessagePackSerializer.Serialize)) &&
                                                             x.IsGenericMethod &&
                                                             x.GetParameters().Length == 1)
                                                 .MakeGenericMethod(new Type[] { this.MethodInfo.ReturnType });

                    // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                    // ZeroFormatterSerializer.Serialize<TReturnType>(new Class().Method(obj.field1, obj.field2, ...))
                    var excecute = Expression.Call(null, serializeMethod, block);
                    var lambda = Expression.Lambda<Func<OperationContext, byte[]>>(excecute, operationContext);
                    this.function = lambda.Compile();
                    this.OperationAsync = (Func<OperationContext, Task>)this.GetType().GetMethod("Function").CreateDelegate(typeof(Func<OperationContext, Task>), this);

                }
            }

            this.filters = option.GlobalFilters
                .Concat(targetType.GetTypeInfo().GetCustomAttributes<NetStitchFilterAttribute>(true))
                .Concat(targetMethodInfo.GetCustomAttributes<NetStitchFilterAttribute>(true))
                .OrderBy(x => x.Order)
                .ToArray();

            this.OperationAsync = SetFilter(this.OperationAsync);

        }

        Func<OperationContext, Task> SetFilter(Func<OperationContext, Task> methodBody)
        {
            foreach (var filter in this.filters.Reverse())
            {
                var fields = filter.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var newFilter = (NetStitchFilterAttribute)Activator.CreateInstance(filter.GetType(), new object[] { methodBody });
                foreach (var item in fields)
                {
                    item.SetValue(newFilter, item.GetValue(filter));
                }
                methodBody = newFilter.Invoke;
            }
            return methodBody;
        }

        public static async Task AsyncAction(OperationContext operationContext, Task task)
        {
            await task.ConfigureAwait(false);
            operationContext.HttpContext.Response.StatusCode = HttpStatus.NoContent;
        }

        public static async Task AsyncFunction<TReturnType>(OperationContext operationContext, Task<TReturnType> task)
        {
            TReturnType result = await task.ConfigureAwait(false);
            HttpResponse responce = operationContext.HttpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;
            MessagePackSerializer.Serialize<TReturnType>(responce.Body, result);
        }

        public async Task Action(OperationContext Context)
        {
            action(Context);
            Context.HttpContext.Response.StatusCode = HttpStatus.NoContent;
            await TaskDone.Done;
        }

        public async Task Function(OperationContext Context)
        {

            byte[] result = function(Context);

            HttpResponse responce = Context.HttpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;

            var ms = responce.Body as System.IO.MemoryStream;
            if (ms != null && ms.Position == 0)
            {
#if NETSTANDERD
                ArraySegment<byte> buf;
                if (ms.TryGetBuffer(out buf))
                {
                    ms.SetLength(result.Length);
                    Buffer.BlockCopy(result, 0, buf.Array, 0, result.Length);
                    return;
                }
#else
                ms.SetLength(result.Length);
                var dest = ms.GetBuffer();
                Buffer.BlockCopy(result, 0, dest, 0, result.Length);
                return;
#endif
            }

            await responce.Body.WriteAsync(result, 0, result.Length);

        }

        public static class TaskDone
        {
            public static Task Done => Task.FromResult(1);
        }
    }
}