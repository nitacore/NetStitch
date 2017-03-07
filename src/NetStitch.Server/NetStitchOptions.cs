using NetStitch.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetStitch.Option
{
    public class NetStitchOption
    {
        public NetStitchOption() { }
        public INetStitchLogger Logger { get; set; }
        public NetStitchFilterAttribute[] GlobalFilters { get; set; }
        public ExceptionHandling ExceptionHandling { get; set; } = ExceptionHandling.HideMessage;
    }

    public enum ExceptionHandling
    {
        HideMessage,
        ShowMessage
    }
}
