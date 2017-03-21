using System;
using System.Collections.Generic;
using System.Text;
using NetStitch.Server;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using NetStitch.Option;
using MessagePack;

namespace NetStitch
{
    public class OperationContext
    {

        private OperationController operationController;

        private NetStitchOption option;

        private HttpContext httpcontext;

        public HttpContext HttpContext => httpcontext;

        public Type ClassType => operationController.ClassType;

        public Type InterfaceType => operationController.InterfaceType;

        public MethodInfo MethodInfo => operationController.MethodInfo;

        public Type ParameterStructType => operationController.ParameterStructType;

        public string OperationID => operationController.OperationID;

        public IFormatterResolver FormatterResolver => option.FormatterResolver;

        public OperationContext(HttpContext httpContext, OperationController operationController, NetStitchOption option)
        {
            this.httpcontext = httpContext;
            this.operationController = operationController;
            this.option = option;
        }

    }
}