using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace NetStitch.Server
{
    public class OperationExecuter
    {
        public static async Task AsyncExecute<TReturnType>(HttpContext httpContext, Task<TReturnType> task)
        {
            TReturnType result = await task.ConfigureAwait(false);
            HttpResponse responce = httpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;
            ZeroFormatterSerializer.Serialize<TReturnType>(responce.Body, result);
        }
        public static async Task AsyncExecute(HttpContext httpContext, Task task)
        {
            await task.ConfigureAwait(false);
            httpContext.Response.StatusCode = HttpStatus.NoContent;
        }
        public static void Execute<TReturnType>(HttpContext httpContext, TReturnType result)
        {
            HttpResponse responce = httpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;
            ZeroFormatterSerializer.Serialize<TReturnType>(responce.Body, result);
        }
    }
}
