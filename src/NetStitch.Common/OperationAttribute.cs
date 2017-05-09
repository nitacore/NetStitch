using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetStitch
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OperationAttribute : Attribute { }
}
