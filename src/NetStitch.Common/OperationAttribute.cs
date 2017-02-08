using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetStitch
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OperationAttribute : Attribute
    {
        public string OperationID { get; set; }
        public OperationAttribute(string OperationID)
        {
            if(string.IsNullOrWhiteSpace(OperationID)) throw new ArgumentException();
            this.OperationID = OperationID;
        }
    }
}
