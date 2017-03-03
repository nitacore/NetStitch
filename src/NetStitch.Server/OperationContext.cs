using System;
using System.Collections.Generic;
using System.Text;
using NetStitch.Server;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace NetStitch
{
    public class OperationContext
    {

        private HttpContext httpContext;

        public HttpContext HttpContext => httpContext;

        private OperationController operationController;

        public Type ClassType => operationController.ClassType;

        public Type InterfaceType => operationController.InterfaceType;

        public MethodInfo MethodInfo => operationController.MethodInfo;

        public Type ParameterStructType => operationController.ParameterStructType;

        public string OperationID => operationController.OperationID;

        public OperationContext(HttpContext httpContext, OperationController operationController)
        {
            this.httpContext = httpContext;
            this.operationController = operationController;
        }

    }
}