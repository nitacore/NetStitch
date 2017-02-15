using System.Collections.Generic;
using System.Threading;
using SharedInterface;
using System;
using SharedProjectValueTuple;

namespace NetStitchClientConsole.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new NetStitch.NetStitchClient("http://localhost:54321");

            var result = client.Create<SharedInterface.IEcho>().EchoAsync("hoge", 1, 2, SharedInterface.MyEnum.A).Result;

            var test = client.Create<ISharedInterfaceValueTuple>().TallyAsync(new[] { (1, 2), (3, 4) }).Result;

            Console.WriteLine($"Sum:{test.sum}, Count: {test.count}");

            Console.WriteLine(result.Name);
            Console.ReadLine();
        }
    }
}