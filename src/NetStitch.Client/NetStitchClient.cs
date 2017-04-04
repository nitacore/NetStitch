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
        readonly protected IFormatterResolver formatterResolver;

        private readonly IDictionary<RuntimeTypeHandle, object> operationDic =
            new Dictionary<RuntimeTypeHandle, object>(new RuntimeTypeHandleEqualityComparer());

        static NetStitchClient()
        {
        }

        public NetStitchClient(string endpoint)
            : this(endpoint, new HttpClient(), MessagePack.Resolvers.StandardResolver.Instance) { }

        public NetStitchClient(string endpoint, HttpClient client)
            : this(endpoint, client, MessagePack.Resolvers.StandardResolver.Instance) { }

        public NetStitchClient(string endpoint, HttpMessageHandler handler)
            : this(endpoint, new HttpClient(handler, false), MessagePack.Resolvers.StandardResolver.Instance) { }

        public NetStitchClient(string endpoint, IFormatterResolver formatterResolver)
            : this(endpoint, new HttpClient(), formatterResolver) { }

        public NetStitchClient(string endpoint, HttpMessageHandler handler, IFormatterResolver formatterResolver)
            : this(endpoint, new HttpClient(handler, false), formatterResolver) { }

        public NetStitchClient(string endpoint, HttpClient client, IFormatterResolver formatterResolver)
        {
            this.endpoint = endpoint.TrimEnd('/');
            this.client = client;
            this.formatterResolver = formatterResolver;
        }

        public T Create<T>()
            where T : INetStitchContract
        {
            var type = typeof(T);

            //Bad Pattern : High Cost
            object result;
            if (!operationDic.TryGetValue(type.TypeHandle, out result))
            {
                result = Activator.CreateInstance(DynamicType<T>.Type, this);
                lock (operationDic)
                {
                    if (!operationDic.ContainsKey(type.TypeHandle))
                        operationDic.Add(type.TypeHandle, result);
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
            return LZ4MessagePackSerializer.Deserialize<T>(bytes, formatterResolver);
        }

        private class DynamicType<T>
        {
            static DynamicType()
            {
                Type = CreateType(typeof(T));
            }

            internal static Type Type { get; private set; }

            private DynamicType() { }
        }

        private static Type CreateType(Type interfaceType)
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

            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] {
                    typeof(NetStitchClient),
                });

            var myConstructorIL = ctor.GetILGenerator();

            //NetStitchClient
            myConstructorIL.Emit(OpCodes.Ldarg_0);
            myConstructorIL.Emit(OpCodes.Ldarg_1);
            myConstructorIL.Emit(OpCodes.Stfld, clientField);
            myConstructorIL.Emit(OpCodes.Ret);

            var info = interfaceType.GetRuntimeMethods()
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

            return typeBuilder.CreateTypeInfo().AsType();

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

        static ConstructorInfo constructorMediaTypeHeaderValue => typeof(System.Net.Http.Headers.MediaTypeHeaderValue)
            .GetTypeInfo().DeclaredConstructors
            .First(x => { var p = x.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(string); });

        static MethodInfo setContentType = typeof(System.Net.Http.Headers.HttpContentHeaders).GetRuntimeProperty("ContentType").SetMethod;

        static ConstructorInfo constructorByteArrayContent => typeof(ByteArrayContent).GetTypeInfo()
            .DeclaredConstructors.First(x => x.GetParameters().Length > 1);

        static ConstructorInfo constructorMessagePackObjectAttribute = typeof(MessagePackObjectAttribute).GetTypeInfo()
            .DeclaredConstructors.First(x => x.GetParameters().Length > 0);

        static ConstructorInfo constructorMessagePackKeyAttribute = typeof(KeyAttribute).GetTypeInfo()
            .DeclaredConstructors.First(x => { var p = x.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(int); });

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

            var pCtor = methodParameterType.GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length > 0);

            var serializer = typeof(LZ4MessagePackSerializer).GetRuntimeMethods()
                .First(x => x.Name == (nameof(LZ4MessagePackSerializer.Serialize)) && x.IsGenericMethod && x.GetParameters().Length == 1)
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

            il.Emit(OpCodes.Newobj, constructorByteArrayContent);

            var byteArrayContent = il.DeclareLocal(typeof(ByteArrayContent));
            il.Emit(OpCodes.Stloc, byteArrayContent);
            il.Emit(OpCodes.Ldloc, byteArrayContent);

            //Set ContentType
            il.Emit(OpCodes.Callvirt, typeof(ByteArrayContent).GetRuntimeProperty("Headers").GetMethod);
            il.Emit(OpCodes.Ldstr, "application/octet-stream");
            il.Emit(OpCodes.Newobj, constructorMediaTypeHeaderValue);
            il.Emit(OpCodes.Callvirt, setContentType);

            il.Emit(OpCodes.Ldloc, byteArrayContent);

            //operationID
            il.Emit(OpCodes.Ldstr, info.OperationID);

            //cancellationToken
            il.Emit(OpCodes.Ldarg_S, info.FullParameters.Length);

            //PostAsync or PostAsync<T>
            il.Emit(OpCodes.Callvirt, info.ReturnInnerType == null ?
               typeof(NetStitchClient).GetTypeInfo().DeclaredMethods.First(x => x.Name == "PostAsync" && !x.IsGenericMethod) :
               typeof(NetStitchClient).GetTypeInfo().DeclaredMethods.First(x => x.Name == "PostAsync" && x.IsGenericMethod)
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

            typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(constructorMessagePackObjectAttribute, new object[] { false }));

            var il = ctor.GetILGenerator();

            var seq = parameterInfo.Select((x, index) => new { name = x.Name, parameterType = x.ParameterType, index });

            foreach (var item in seq)
            {
                var field = typeBuilder.DefineField(item.name, item.parameterType, FieldAttributes.Public);
                field.SetCustomAttribute(new CustomAttributeBuilder(constructorMessagePackKeyAttribute, new object[] { item.index }));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_S, item.index + 1);
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo().AsType();

        }

    }

    public class RuntimeTypeHandleEqualityComparer : IEqualityComparer<RuntimeTypeHandle>
    {
        public bool Equals(RuntimeTypeHandle x, RuntimeTypeHandle y)
            => x.Equals(y);

        public int GetHashCode(RuntimeTypeHandle obj)
            => obj.GetHashCode();
    }

}
