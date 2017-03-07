using System;
using System.Collections.Generic;
using System.Text;
using NetStitch.Server;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using NetStitch.Option;

namespace NetStitch
{
    public class OperationContext
    {

        private HttpContext httpcontext;

        public HttpContext HttpContext => httpcontext;

        private OperationController operationController;

        public Type ClassType => operationController.ClassType;

        public Type InterfaceType => operationController.InterfaceType;

        public MethodInfo MethodInfo => operationController.MethodInfo;

        public Type ParameterStructType => operationController.ParameterStructType;

        public string OperationID => operationController.OperationID;

        public NetStitchOption Opetion;

        public OperationContext(HttpContext httpContext, OperationController operationController, NetStitchOption option)
        {
            this.httpcontext = httpContext;
            this.operationController = operationController;
            this.Opetion = option;
        }

    }
}