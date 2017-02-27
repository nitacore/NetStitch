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
        public readonly Type ParameterStructType;

        private readonly OperationType operationType;
        private readonly Action<OperationContext> operation;
        private readonly Func<OperationContext, Task> asyncOperation;

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

        public OperationController(Type targetType, Type interfaceType, MethodInfo targetMethodInfo, MethodInfo interfaceMethodInfo)
        {

            this.ClassType = targetType;
            this.InterfaceType = interfaceType;
            this.MethodInfo = targetMethodInfo;
            this.ParameterStructType = CreateParameterSturctType();
            this.OperationID = ((OperationAttribute)interfaceMethodInfo.GetCustomAttribute(typeof(OperationAttribute))).OperationID;

            bool requiresHttpContext = targetType.GetInterfaces().Any(x => x == typeof(IHttpContext));

            bool operationIsAsyncType = typeof(Task).IsAssignableFrom(targetMethodInfo.ReturnType);

            bool operationIsAsyncFunction = operationIsAsyncType && targetMethodInfo.ReturnType.GenericTypeArguments.Length != 0;

            Type asyncRetunType = targetMethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault();

            MethodInfo deserializeMethod = typeof(ZeroFormatterSerializer).GetMethods()
                                           .First(x => x.Name == (nameof(ZeroFormatterSerializer.Deserialize)) &&
                                                       x.IsGenericMethod &&
                                                       x.GetParameters().Any(p => p.ParameterType == typeof(System.IO.Stream)))
                                           .MakeGenericMethod(new Type[] { ParameterStructType });

            // Context
            var operationContext = Expression.Parameter(typeof(OperationContext), nameof(OperationContext));
            var httpContext = Expression.Property(operationContext, nameof(HttpContext));
            var bindContext = Expression.Bind(typeof(IHttpContext).GetProperty(nameof(IHttpContext.Context)), httpContext);

            // new Class() or new Class() { Context = Context }
            var newClass = requiresHttpContext ?
            Expression.MemberInit(Expression.New(targetType), bindContext) :
            Expression.MemberInit(Expression.New(targetType));

            // Context.Request.Body
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
                    typeof(OperationExecuter).GetMethods().First(x => x.Name == (nameof(OperationExecuter.AsyncExecute)) && x.IsGenericMethod).MakeGenericMethod(asyncRetunType) :
                    typeof(OperationExecuter).GetMethods().First(x => x.Name == (nameof(OperationExecuter.AsyncExecute)) && !x.IsGenericMethod);

                var taskExecute = Expression.Call(null, asyncExecuteMethodInfo, operationContext, block);
                var lambda = Expression.Lambda<Func<OperationContext, Task>>(taskExecute, operationContext);
                asyncOperation = lambda.Compile();
                this.operationType = OperationType.AsyncOperation;
            }
            else
            {
                // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                // new Class().Method(obj.field1, obj.field2, ...)
                if (targetMethodInfo.ReturnType == typeof(void))
                {
                    var lambda = Expression.Lambda<Action<OperationContext>>(block, operationContext);
                    this.operation = lambda.Compile();
                    this.operationType = OperationType.Action;
                }
                else
                {
                    // ParameterStructType obj = Zeroformatter.Deserialize<ParameterStructType>(HttpContext.Request.Body);
                    // Execute(new Class().Method(obj.field1, obj.field2, ...))
                    var executeMethodInfo = typeof(OperationExecuter).GetMethods()
                        .First(x => x.Name == nameof(OperationExecuter.Execute) && x.IsGenericMethod)
                        .MakeGenericMethod(targetMethodInfo.ReturnType);
                    var excecute = Expression.Call(null, executeMethodInfo, operationContext, block);
                    var lambda = Expression.Lambda<Action<OperationContext>>(excecute, operationContext);
                    this.operation = lambda.Compile();
                    this.operationType = OperationType.Operation;
                }
            }
        }

        public async Task ExecuteAsync(OperationContext operationContext)
        {
            switch (operationType)
            {
                case OperationType.Action:
                    operation(operationContext);
                    operationContext.HttpContext.Response.StatusCode = HttpStatus.NoContent;
                    break;
                case OperationType.Operation:
                    operation(operationContext);
                    break;
                case OperationType.AsyncOperation:
                    await asyncOperation(operationContext).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException("operation not found");
            }
        }

    }
}
