using System;

namespace NetStitchClientConsole.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = new NetStitch.NetStitchClient("http://localhost:54321").Create<SharedInterface.IEcho>().EchoAsync("hoge", 1, 2, SharedInterface.MyEnum.A).Result;
            Console.WriteLine(result.Name);
            Console.ReadLine();
        }
    }
}