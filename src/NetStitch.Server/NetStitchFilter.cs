using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetStitch
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public abstract class NetStitchFilterAttribute : Attribute
    {

        public int Order { get; set; } = int.MaxValue;

        protected Func<OperationContext, Task> Next { get; private set; }

        public NetStitchFilterAttribute(Func<OperationContext, Task> next)
        {
            Next = next;
        }

        public abstract Task Invoke(OperationContext context);

    }
}