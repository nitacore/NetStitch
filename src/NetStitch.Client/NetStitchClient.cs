using MessagePack;
using NetStitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetStitch
{
    public class NetStitchClient
    {
        readonly protected string endpoint;
        readonly protected HttpClient client;

        private readonly IDictionary<Type, object> operationDic = new Dictionary<Type, object>();

        static NetStitchClient()
        {
        }

        public NetStitchClient(string endpoint)
                : this(endpoint, new HttpClient())
            { }

        public NetStitchClient(string endpoint, HttpClient client)
        {
            this.endpoint = endpoint.TrimEnd('/');
            this.client = client;
        }

        public T Create<T>()
        {
            var type = typeof(T);

            //Bad Pattern : High Cost
            object result;
            if (!operationDic.TryGetValue(type, out result))
            {
                var ctorInfo = StubType<T>.Value.GetConstructor(new Type[] { typeof(NetStitchClient) });
                result = ctorInfo.Invoke(new object[] { this });
                lock (operationDic)
                {
                    if (!operationDic.ContainsKey(type))
                        operationDic.Add(type, result);
                }
            }
            return (T)result;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual async Task PostAsync(HttpContent content, string operationID, CancellationToken cancellationToken)
        {
            var response = await client.PostAsync(endpoint + "/" + operationID, content, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public virtual async Task<T> PostAsync<T>(HttpContent content, string operationID, CancellationToken cancellationToken)
        {
            var response = await client.PostAsync(endpoint + "/" + operationID, content, cancellationToken).ConfigureAwait(false);
            var bytes = await response.EnsureSuccessStatusCode().Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        private class StubType<T>
        {
            private static readonly Lazy<TypeInfo> lazy = new Lazy<TypeInfo>(() => (CreateType(typeof(T))));

            internal static TypeInfo Value { get { return lazy.Value; } }

            private StubType() { }
        }

        private static TypeInfo CreateType(Type interfaceType)
        {
            var typeSignature = "___" + interfaceType.Name;
            var assemblyName = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(interfaceType.Name);
            var typeBuilder = moduleBuilder.DefineType(typeSignature,
                                TypeAttributes.Public |
                                TypeAttributes.Class
                                //TypeAttributes.AutoClass |
                                //TypeAttributes.AnsiClass |
                                //TypeAttributes.BeforeFieldInit |
                                //TypeAttributes.AutoLayout
                                , null, new Type[] { interfaceType });
            typeBuilder.AddInterfaceImplementation(interfaceType);

            var clientField = typeBuilder.DefineField("_client", typeof(NetStitchClient), FieldAttributes.Private);

            var aCtor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] {
                    typeof(NetStitchClient),
                });

            var myConstructorIL = aCtor.GetILGenerator();

            //NetStitchClient
            myConstructorIL.Emit(OpCodes.Ldarg_0);
            myConstructorIL.Emit(OpCodes.Ldarg_1);
            myConstructorIL.Emit(OpCodes.Stfld, clientField);
            myConstructorIL.Emit(OpCodes.Ret);

            var info = interfaceType.GetTypeInfo().GetMethods()
            .Select(x =>
            new
            {
                ((OperationAttribute)x.GetCustomAttribute(typeof(OperationAttribute)))?.OperationID,
                methodInfo = x
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.OperationID))
            .Select(x => new OperationInfo()
            {
                InterfaceType = interfaceType,
                MethodInfo = x.methodInfo,
                OperationID = x.OperationID,
                FullParameters = x.methodInfo.GetParameters(),
                Parameters = x.methodInfo.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken)).ToArray(),
                ReturnType = x.methodInfo.ReturnType,
                ReturnInnerType = x.methodInfo.ReturnType.GenericTypeArguments.Length == 0 ? null :
                                  x.methodInfo.ReturnType.GenericTypeArguments[0],
            });
            foreach (var method in info)
            {
                var mb = CreateMethod(method, typeBuilder, clientField);
                typeBuilder.DefineMethodOverride(mb, method.MethodInfo);
            }

            return typeBuilder.CreateTypeInfo();

        }

        internal class OperationInfo
        {
            public Type InterfaceType;
            public MethodInfo MethodInfo;
            public string OperationID;
            public ParameterInfo[] FullParameters;
            public ParameterInfo[] Parameters;
            public Type ReturnInnerType;
            public Type ReturnType;
        }

        private static MethodBuilder CreateMethod(OperationInfo info, TypeBuilder thisType, FieldBuilder clientField)
        {

            var method = thisType.DefineMethod(
                info.MethodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                    info.ReturnType,
                    info.FullParameters.Select(y => y.ParameterType).ToArray()
                );
            var il = method.GetILGenerator();

            //NetStitchClient
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, clientField);

            //HttpContent
            var bytes = il.DeclareLocal(typeof(byte[]));

            var methodParameterType = CreateParameterSturctType(info.InterfaceType, info.MethodInfo, info.Parameters);

            var pCtor = methodParameterType.GetTypeInfo().GetConstructor(info.Parameters.Select(x => x.ParameterType).ToArray());

            var serializer = typeof(MessagePackSerializer).GetTypeInfo().GetMethods()
                .First(x => x.Name == (nameof(MessagePackSerializer.Serialize)) && x.IsGenericMethod && x.GetParameters().Length == 1)
                .MakeGenericMethod(methodParameterType);

            //ByteArrayContent
            for (int i = 0; i < info.Parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i + 1);  // arg0 = this
            }

            il.Emit(OpCodes.Newobj, pCtor);

            il.Emit(OpCodes.Call, serializer);

            il.Emit(OpCodes.Stloc, bytes);
            il.Emit(OpCodes.Ldloc, bytes);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldloc, bytes);
            il.Emit(OpCodes.Ldlen);

            il.Emit(OpCodes.Newobj, typeof(ByteArrayContent).GetTypeInfo().GetConstructor(new[] { typeof(byte[]), typeof(int), typeof(int) }));

            var byteArrayContent = il.DeclareLocal(typeof(ByteArrayContent));
            il.Emit(OpCodes.Stloc, byteArrayContent);
            il.Emit(OpCodes.Ldloc, byteArrayContent);

            //Set ContentType
            il.Emit(OpCodes.Callvirt, typeof(ByteArrayContent).GetTypeInfo().GetMethod("get_Headers", BindingFlags.Public | BindingFlags.Instance));
            il.Emit(OpCodes.Ldstr, "application/octet-stream");
            il.Emit(OpCodes.Newobj, typeof(System.Net.Http.Headers.MediaTypeHeaderValue).GetTypeInfo().GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Callvirt, typeof(System.Net.Http.Headers.HttpContentHeaders).GetTypeInfo().GetMethod("set_ContentType", BindingFlags.Public | BindingFlags.Instance));

            il.Emit(OpCodes.Ldloc, byteArrayContent);

            //operationID
            il.Emit(OpCodes.Ldstr, info.OperationID);

            //cancellationToken
            il.Emit(OpCodes.Ldarg_S, info.FullParameters.Length);

            //PostAsync or PostAsync<T>
            il.Emit(OpCodes.Callvirt, info.ReturnInnerType == null ?
               typeof(NetStitchClient).GetTypeInfo().GetMethods().First(x => x.Name == "PostAsync" && !x.IsGenericMethod) :
               typeof(NetStitchClient).GetTypeInfo().GetMethods().First(x => x.Name == "PostAsync" && x.IsGenericMethod)
                  .MakeGenericMethod(new[] { info.ReturnInnerType }));
            il.Emit(OpCodes.Ret);

            return method;
        }

        private static Type CreateParameterSturctType(Type interfaceType, MethodInfo methodInfo, ParameterInfo[] parameterInfo)
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
                parameterInfo.Select(x => x.ParameterType).ToArray()
                );

            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(MessagePackObjectAttribute).GetTypeInfo().GetConstructor(new Type[] { typeof(bool) }), new object[] { false }));

            var il = ctor.GetILGenerator();

            var seq = parameterInfo.Select((x, index) => new { name = x.Name, parameterType = x.ParameterType, index });

            foreach (var item in seq)
            {
                var field = typeBuilder.DefineField(item.name, item.parameterType, FieldAttributes.Public);
                field.SetCustomAttribute(new CustomAttributeBuilder(typeof(KeyAttribute).GetTypeInfo().GetConstructor(new[] { typeof(int) }), new object[] { item.index }));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, item.index + 1);
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo().AsType();

        }
    }
}
