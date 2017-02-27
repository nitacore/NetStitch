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
        public static async Task AsyncExecute<TReturnType>(OperationContext operationContext, Task<TReturnType> task)
        {
            TReturnType result = await task.ConfigureAwait(false);
            HttpResponse responce = operationContext.HttpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;
            ZeroFormatterSerializer.Serialize<TReturnType>(responce.Body, result);
        }
        public static async Task AsyncExecute(OperationContext operationContext, Task task)
        {
            await task.ConfigureAwait(false);
            operationContext.HttpContext.Response.StatusCode = HttpStatus.NoContent;
        }
        public static void Execute<TReturnType>(OperationContext operationContext, TReturnType result)
        {
            HttpResponse responce = operationContext.HttpContext.Response;
            responce.ContentType = "application/octet-stream";
            responce.StatusCode = HttpStatus.OK;
            ZeroFormatterSerializer.Serialize<TReturnType>(responce.Body, result);
        }
    }
}
