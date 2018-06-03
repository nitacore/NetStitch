using Microsoft.AspNetCore.Http;
using NetStitch.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.IO;
using System.Text;
using NetStitch.Swagger;

namespace NetStitch.Swagger
{
    public class NetStitchSwaggerMiddleware
    {
        static NetStitchSwaggerMiddleware()
        {
        }

        public NetStitchSwaggerMiddleware(RequestDelegate next)
        {
            this.next = next;
            this.options = new SwaggerOptions("NetStitch.Server", "Swagger Integration Test", "/");
        }

        public NetStitchSwaggerMiddleware(RequestDelegate next, SwaggerOptions options)
        {
            this.next = next;
            this.options = options;
        }

        private readonly RequestDelegate next;

        internal readonly SwaggerOptions options;

        public async Task Invoke(HttpContext httpContext)
        {
            if (!NetStitchMiddleware.Servers.TryGetValue(options.ServerID, out var server))
            {
                httpContext.Response.ContentType = "text/plain";
                httpContext.Response.StatusCode = HttpStatus.NotFound;
                byte[] bytes = Encoding.UTF8.GetBytes($"ServerID Not Found : {options.ServerID}");
                httpContext.Response.Body.Write(bytes, 0, bytes.Length);
                return;
            }

            if (httpContext.Request.ContentType == "application/x-msgpack")
            {
                await next(httpContext).ConfigureAwait(false);
            }
            else if (httpContext.Request.ContentType == "application/x-www-form-urlencoded")
            {
                if (server.OperationMap.TryGetValue(httpContext.Request.Path, out var operation))
                {
                    using (var stream = new NonDisposableStream(new MemoryStream()))
                    {
                        var originalStream = httpContext.Response.Body;
                        httpContext.Response.Body = stream;

                        await CallOperationFromContentTypeOfFormUrlencoded(httpContext, server, operation).ConfigureAwait(false);

                        if (httpContext.Response.StatusCode == HttpStatus.OK)
                        {
                            httpContext.Response.ContentType = "application/json";
                            httpContext.Response.Headers.Remove("Content-Encording");
                            var returnType = operation.MethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault();
                            var obj = MessagePack.LZ4MessagePackSerializer.NonGeneric.Deserialize(returnType, stream);
                            using (var sw = new StreamWriter(originalStream, Encoding.UTF8))
                            using (var jw = new JsonTextWriter(sw))
                            {
                                JsonSerializer.Create().Serialize(jw, obj);
                            }
                        }
                        else
                        {
                            stream.Position = 0;
                            stream.CopyTo(originalStream);
                        }
                    }
                }
            }
            else if (httpContext.Request.ContentType == "application/json")
            {
                if (server.OperationMap.TryGetValue(httpContext.Request.Path, out var operation))
                {
                    using (var stream = new NonDisposableStream(new MemoryStream()))
                    {
                        var originalStream = httpContext.Response.Body;
                        httpContext.Response.Body = stream;

                        await CallOperationFromContentTypeOfJson(httpContext, server, operation).ConfigureAwait(false);

                        if (httpContext.Response.StatusCode == HttpStatus.OK)
                        {
                            httpContext.Response.ContentType = "application/json";
                            httpContext.Response.Headers.Remove("Content-Encording");
                            var returnType = operation.MethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault();
                            var obj = MessagePack.LZ4MessagePackSerializer.NonGeneric.Deserialize(returnType, stream);
                            using (var sw = new StreamWriter(originalStream, Encoding.UTF8))
                            using (var jw = new JsonTextWriter(sw))
                            {
                                JsonSerializer.Create().Serialize(jw, obj);
                            }
                        }
                        else
                        {
                            stream.Position = 0;
                            stream.CopyTo(originalStream);
                        }
                    }
                }
            }
            else
            {
                await CallSwagger(httpContext, server).ConfigureAwait(false);
            }
        }

        public Task CallSwagger(HttpContext httpContext, NetStitchServer server)
        {
            const string prefix = "NetStitch.Swagger.SwaggerUI.";

            var path = httpContext.Request.Path.Value.Trim('/');
            if (path == "") path = "index.html";
            var filePath = prefix + path.Replace("/", ".");

            if (MediaTypeDictionary.TryGetValue(filePath.Split('.').Last(), out string mediaType))
            {
                if (path.EndsWith(options.JsonName))
                {
                    var builder = new SwaggerBuilder(options, httpContext);
                    var bytes = builder.BuildSwagger(server);
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = HttpStatus.OK;
                    httpContext.Response.Body.Write(bytes, 0, bytes.Length);
                    return Task.CompletedTask;
                }

                var myAssembly = typeof(NetStitchSwaggerMiddleware).GetTypeInfo().Assembly;

                using (var stream = myAssembly.GetManifestResourceStream(filePath))
                {
                    if (stream == null)
                    {
                        return next(httpContext);
                    }
                    httpContext.Response.ContentType = mediaType;
                    httpContext.Response.StatusCode = HttpStatus.OK;
                    var response = httpContext.Response.Body;
                    stream.CopyTo(response);
                }
            };

            return Task.CompletedTask;
        }

        public Task CallOperationFromContentTypeOfFormUrlencoded(HttpContext httpContext, NetStitchServer server, OperationController operation)
        {

            var args = new List<object>();
            object obj;
            var typeArgs = new List<Type>();
            try
            {
                foreach (var methodParameter in operation.MethodInfo.GetParameters())
                {

                    typeArgs.Add(methodParameter.ParameterType);

                    if (httpContext.Request.Form.TryGetValue(methodParameter.Name, out var stringValues))
                    {
                        if (methodParameter.ParameterType == typeof(string))
                        {
                            args.Add((string)stringValues);
                        }
                        else if (methodParameter.ParameterType.GetTypeInfo().IsEnum)
                        {
                            args.Add(Enum.Parse(methodParameter.ParameterType, (string)stringValues));
                        }
                        else
                        {
                            var collectionType = GetCollectionType(methodParameter.ParameterType);
                            if (collectionType == null || stringValues.Count == 1)
                            {
                                var values = (string)stringValues;

                                if (methodParameter.ParameterType == typeof(DateTime) ||
                                    methodParameter.ParameterType == typeof(DateTimeOffset) ||
                                    methodParameter.ParameterType == typeof(DateTime?) ||
                                    methodParameter.ParameterType == typeof(DateTimeOffset?))
                                {
                                    values = "\"" + values + "\"";
                                }

                                args.Add(JsonConvert.DeserializeObject(values, methodParameter.ParameterType));
                            }
                            else
                            {
                                string serializeTarget;
                                if (collectionType == typeof(string))
                                {
                                    serializeTarget = $"[{string.Join(", ", stringValues.Select(x => JsonConvert.SerializeObject(x)))}]"; 
                                }
                                else if (collectionType.GetTypeInfo().IsEnum || 
                                         collectionType == typeof(DateTime)  || 
                                         collectionType == typeof(DateTimeOffset) || 
                                         collectionType == typeof(DateTime?) || 
                                         collectionType == typeof(DateTimeOffset?))
                                {
                                    serializeTarget = $"[{string.Join(", ", stringValues.Select(x => "\"" + x + "\""))}]";
                                }
                                else
                                {
                                    serializeTarget = $"[{(string)stringValues}]";
                                }

                                args.Add(JsonConvert.DeserializeObject(serializeTarget, methodParameter.ParameterType));
                            }
                        }
                    }
                    else
                    {
                        if (methodParameter.HasDefaultValue)
                        {
                            args.Add(methodParameter.DefaultValue);
                        }
                        else
                        {
                            args.Add(null);
                        }
                    }
                }
                obj = Activator.CreateInstance(operation.ParameterType, args.ToArray());
                httpContext.Request.Body = new System.IO.MemoryStream();
                if (obj != null)
                    MessagePack.LZ4MessagePackSerializer.NonGeneric.Serialize(operation.ParameterType, httpContext.Request.Body, obj, server.Option.FormatterResolver);
                return next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = HttpStatus.InternalServerError;
                var bytes = Encoding.UTF8.GetBytes(ex.ToString());
                httpContext.Response.Body.Write(bytes, 0, bytes.Length);
            }
            return Task.CompletedTask;
        }

        public Task CallOperationFromContentTypeOfJson(HttpContext httpContext, NetStitchServer server, OperationController operation)
        {

            try
            {
                var args = new List<object>();
                object obj;
                string body;

                using (var sr = new StreamReader(httpContext.Request.Body, Encoding.UTF8))
                {
                    body = sr.ReadToEnd();
                }
                if (operation.ParameterType == typeof(byte[]) && string.IsNullOrWhiteSpace(body))
                {
                    body = "[]";
                }

                obj = Newtonsoft.Json.JsonConvert.DeserializeObject(body, operation.ParameterType);

                httpContext.Request.Body = new System.IO.MemoryStream();
                if (obj != null)
                    MessagePack.LZ4MessagePackSerializer.NonGeneric.Serialize(operation.ParameterType, httpContext.Request.Body, obj, server.Option.FormatterResolver);

                return next(httpContext);

            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = HttpStatus.InternalServerError;
                var bytes = Encoding.UTF8.GetBytes(ex.ToString());
                httpContext.Response.Body.Write(bytes, 0, bytes.Length);
                return Task.CompletedTask;
            }
        }

        Type GetCollectionType(Type type)
        {
            if (type.IsArray) return type.GetElementType();

            if (type.GetTypeInfo().IsGenericType)
            {
                var genTypeDef = type.GetGenericTypeDefinition();
                if (genTypeDef == typeof(IEnumerable<>)
                || genTypeDef == typeof(ICollection<>)
                || genTypeDef == typeof(IList<>)
                || genTypeDef == typeof(List<>)
                || genTypeDef == typeof(IReadOnlyCollection<>)
                || genTypeDef == typeof(IReadOnlyList<>))
                {
                    return genTypeDef.GetGenericArguments()[0];
                }
            }

            return null;
        }

        static Dictionary<string, string> MediaTypeDictionary = new Dictionary<string, string>()
        {
            {"css", "text/css" },
            {"js", "text/javascript" },
            {"svg", "text/css" },
            {"ico", "text/css" },
            {"otf", "text/css" },
            {"ttf", "text/css" },
            {"woff", "application/font-woff" },
            {"woff2", "text/css" },
            {"eot", "application/vnd.ms-fontobject" },
            {"png", "image/png" },
            {"gif", "image/gif" },
            {"json", "application/json" },
            {"html", "text/html" }
        };
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class NetStitchSwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder builder, SwaggerOptions option)
        {
            return builder.UseMiddleware<NetStitchSwaggerMiddleware>(option);
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NetStitchSwaggerMiddleware>();
        }
    }
}
