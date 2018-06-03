using MessagePack;
using NetStitch.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetStitch.Option
{
    public class NetStitchOption
    {
        public NetStitchOption() { }
        public INetStitchLogger Logger { get; set; } = new NetStitchEmptyLogger();
        public NetStitchFilterAttribute[] GlobalFilters { get; set; } = new NetStitchFilterAttribute[0];
        public ExceptionHandling ExceptionHandling { get; set; } = ExceptionHandling.HideMessage;
        public IFormatterResolver FormatterResolver { get; set; } = MessagePack.Resolvers.StandardResolver.Instance;
        public string ServerID { get; set; } = "default";
    }

    public enum ExceptionHandling
    {
        HideMessage,
        ShowMessage,
        ShowStackTrace
    }
}
