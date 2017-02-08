using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetStitch
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class NetStitchContractAttribute : Attribute { }
}
