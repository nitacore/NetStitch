using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

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

        private readonly OperationType operationType;
        private readonly Action<HttpContext> operation;
        private readonly Func<HttpContext, Task> asyncOperation;

        private Type CreateParameterSturctType()
        {
            var assemblyName = new AssemblyName($"___{InterfaceType.FullName}.{MethodInfo.Name}");

            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var typeBuilder = moduleBuilder.DefineType($"___{InterfaceType.FullName}.{MethodInfo.Name}",
                TypeAttributes.Public |
                TypeAttributes.SequentialLayout |
                TypeAttributes.AnsiClass |
                TypeAttributes.Sealed |
                TypeAttributes.BeforeFieldInit,
                typeof(ValueType));

            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                MethodInfo.GetParameters().Select(x => x.ParameterType).ToArray()
                );

            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(ZeroFormattableAttribute).GetConstructor(Type.EmptyTypes), new object[0]));

            var il = ctor.GetILGenerator();

            var seq = MethodInfo.GetParameters().Select((x, index) => new { name = x.Name, parameterType = x.ParameterType, index });

            foreach (var item in seq)
            {
                var field = typeBuilder.DefineField(item.name, item.parameterType, FieldAttributes.Public);
                field.SetCustomAttribute(new CustomAttributeBuilder(typeof(IndexAttribute).GetConstructor(new[] { typeof(int) }), new object[] { item.index }));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, item.index + 1);
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo().AsType();

        }

        public OperationController(Type classType, Type interfaceType, MethodInfo methodInfo)
        {

            this.ClassType = classType;
            this.InterfaceType = interfaceType;
            this.MethodInfo = methodInfo;

            this.OperationID = ((OperationAttribute)methodInfo.GetCustomAttribute(typeof(OperationAttribute))).OperationID;

            bool requiresHttpContext = classType.GetInterfaces().Any(x => x == typeof(IHttpContext));

            bool operationIsAsyncType = typeof(Task).IsAssignableFrom(methodInfo.ReturnType);

            bool operationIsAsyncFunction = operationIsAsyncType && methodInfo.ReturnType.GenericTypeArguments.Length != 0;

            Type asyncRetunType = methodInfo.ReturnType.GenericTypeArguments.FirstOrDefault();

            Type parameterStructType = CreateParameterSturctType();

            MethodInfo deserializeMethod = typeof(ZeroFormatterSerializer).GetMethods()
                                           .First(x => x.Name == (nameof(ZeroFormatterSerializer.Deserialize)) &&
                                                       x.IsGenericMethod &&
                                                       x.GetParameters().Any(p => p.ParameterType == typeof(System.IO.Stream)))
                                           .MakeGenericMethod(new Type[] { parameterStructType });

            // Context
            var context = Expression.Parameter(typeof(HttpContext), nameof(IHttpContext.Context));
            var bindContext = Expression.Bind(typeof(IHttpContext).GetProperty(nameof(IHttpContext.Context)), context);

            // new Class() or new Class() { Context = Context }
            var newClass = requiresHttpContext ?
            Expression.MemberInit(Expression.New(classType), bindContext) :
            Expression.MemberInit(Expression.New(classType));

            // Context.Request.Body
            var request = Expression.Property(context, nameof(HttpContext.Request));
            var body = Expression.Property(request, nameof(HttpRequest.Body));

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            var deserialize = Expression.Call(null, deserializeMethod, body);
            var obj = Expression.Parameter(parameterStructType, "obj");
            var assign = Expression.Assign(obj, deserialize);

            // obj.field1, obj.field2, ...
            var args = methodInfo.GetParameters().Select(x => Expression.Field(obj, x.Name)).ToArray();

            // ((Interface)new Class())
            var convertInterface = Expression.Convert(newClass, interfaceType);

            // ((Interface)new Class())).Method(obj.field1, obj.field2, ...)
            var callMethod = Expression.Call(convertInterface, methodInfo, args);

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            // ((Interface)new Class())).Method(obj.field1, obj.field2, ...)
            var block = Expression.Block(new[] { obj }, assign, callMethod);

            // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
            // AsyncExecute(((Interface)new Class())).Method(obj.field1, obj.field2, ...))
            if (operationIsAsyncType)
            {
                var asyncExecuteMethodInfo = operationIsAsyncFunction ?
                    typeof(OperationExecuter).GetMethods().First(x => x.Name == (nameof(OperationExecuter.AsyncExecute)) && x.IsGenericMethod).MakeGenericMethod(asyncRetunType) :
                    typeof(OperationExecuter).GetMethods().First(x => x.Name == (nameof(OperationExecuter.AsyncExecute)) && !x.IsGenericMethod);

                var taskExecute = Expression.Call(null, asyncExecuteMethodInfo, context, block);
                var lambda = Expression.Lambda<Func<HttpContext, Task>>(taskExecute, context);
                asyncOperation = lambda.Compile();
                this.operationType = OperationType.AsyncOperation;
            }
            else
            {
                // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                // ((Interface)new Class())).Method(obj.field1, obj.field2, ...)
                if (methodInfo.ReturnType == typeof(void))
                {
                    var lambda = Expression.Lambda<Action<HttpContext>>(block, context);
                    this.operation = lambda.Compile();
                    this.operationType = OperationType.Action;
                }
                else
                {
                    // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                    // Execute(((Interface)new Class())).Method(obj.field1, obj.field2, ...))
                    var executeMethodInfo = typeof(OperationExecuter).GetMethods()
                        .First(x => x.Name == nameof(OperationExecuter.Execute) && x.IsGenericMethod)
                        .MakeGenericMethod(methodInfo.ReturnType);
                    var excecute = Expression.Call(null, executeMethodInfo, context, block);
                    var lambda = Expression.Lambda<Action<HttpContext>>(excecute, context);
                    this.operation = lambda.Compile();
                    this.operationType = OperationType.Operation;
                }
            }
        }

        public async Task ExecuteAsync(HttpContext httpcontext)
        {
            switch (operationType)
            {
                case OperationType.Action:
                    operation(httpcontext);
                    httpcontext.Response.StatusCode = HttpStatus.NoContent;
                    break;
                case OperationType.Operation:
                    operation(httpcontext);
                    break;
                case OperationType.AsyncOperation:
                    await asyncOperation(httpcontext).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException("operation not found");
            }
        }

    }
}
