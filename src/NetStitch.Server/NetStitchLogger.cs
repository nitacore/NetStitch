using System;
using System.Collections.Generic;
using System.Text;

#if NETSTANDARD1_6 || NETCOREAPP1_0
using Microsoft.Extensions.Logging;
#endif

namespace NetStitch.Logger
{

    public interface INetStitchLogger
    {
        void ServerSetupStart();
        void ServerSetupCompleted(TimeSpan elapsed);
        void OperationStart(string interfaceName, string typeName, string method);
        void OperationCompleted(string interfaceName, string typeName, string method, TimeSpan elapsed);
        void OperationError(string interfaceName, string typeName, string method, Exception exception);
    }

    public class NetStitchEmptyLogger : INetStitchLogger
    {
        public NetStitchEmptyLogger() { }

        public void OperationError(string interfaceName, string typeName, string method, Exception exception) { }

        public void OperationCompleted(string interfaceName, string typeName, string method, TimeSpan elapsed) { }

        public void OperationStart(string interfaceName, string typeName, string method) { }

        public void ServerSetupCompleted(TimeSpan elapsed) { }

        public void ServerSetupStart() { }
    }

    public class NetStitchConsoleLogger : INetStitchLogger
    {
        public NetStitchConsoleLogger() { }

        public void OperationError(string interfaceName, string typeName, string method, Exception exception)
        {
            Console.WriteLine($"OperationError : (({interfaceName}){typeName}).{method}");
            Console.WriteLine($"Message : { exception.Message }");
            Console.WriteLine($"StackTrace : { exception.StackTrace }");
        }

        public void OperationCompleted(string interfaceName, string typeName, string method, TimeSpan elapsed)
        {
            Console.WriteLine($"OperationCompleted : (({interfaceName}){typeName}).{method}");
            Console.WriteLine($"    Elapsed : { elapsed }");
        }

        public void OperationStart(string interfaceName, string typeName, string method)
        {
            Console.WriteLine($"OperationStart : (({interfaceName}){typeName}).{method}");
        }

        public void ServerSetupCompleted(TimeSpan elapsed)
        {
            Console.WriteLine($"ServerSetupCompleted");
            Console.WriteLine($"    Elapsed : { elapsed }");
        }

        public void ServerSetupStart()
        {
            Console.WriteLine($"ServerSetupStart");
        }
    }

#if NETSTANDARD1_6 || NETCOREAPP1_0

    public class NetStitchLogger : INetStitchLogger
    {

        readonly Microsoft.Extensions.Logging.ILogger logger;

        enum NetStitchEvent
        {
            ServerSetupStart,
            ServerSetupCompleted,
            OperationInvokeStart,
            OperationInvokeCompleted,
            OperationError
        }

        public NetStitchLogger(Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
        }

        public void OperationError(string interfaceName, string typeName, string method, Exception exception)
        {
            logger.LogInformation((int)NetStitchEvent.OperationError, exception, nameof(NetStitchEvent.OperationError));
        }

        public void OperationCompleted(string interfaceName, string typeName, string method, TimeSpan elapsed)
        {
            logger.LogInformation((int)NetStitchEvent.OperationInvokeCompleted, nameof(NetStitchEvent.OperationInvokeCompleted) + " in {0} ms", elapsed.Milliseconds, elapsed);
        }

        public void OperationStart(string interfaceName, string typeName, string method)
        {
            logger.LogInformation((int)NetStitchEvent.OperationInvokeStart, nameof(NetStitchEvent.OperationInvokeStart));
        }

        public void ServerSetupCompleted(TimeSpan elapsed)
        {
            logger.LogInformation((int)NetStitchEvent.ServerSetupCompleted, nameof(NetStitchEvent.ServerSetupCompleted) + " in {0} ms", elapsed.Milliseconds);
        }

        public void ServerSetupStart()
        {
            logger.LogInformation((int)NetStitchEvent.ServerSetupStart, nameof(NetStitchEvent.ServerSetupStart));
        }

    }
#endif

}
