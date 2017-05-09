using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStitchClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        static NetStitch.NetStitchClient client = new NetStitch.NetStitchClient("http://localhost:54321");

        static async Task MainAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {

                var stub = client.Create<SharedProjectValueTask.IValueTask>();

                //var test = await stub.TallyAsync(1, 3);
                var seq = await Task.WhenAll(Enumerable.Range(1, 1000)
                    .Select(x => stub.TallyAsync(x, x + 10).AsTask()
                    ));

                foreach (var item in seq)
                {
                    Console.WriteLine(item);
                }

                //var ComplexType = await client.Create<SharedInterface.IComplexType>().EchoAsync(new SharedInterface.MyClass() { Name = "Complex", Sum = 11 });

                //var stub = client.Create<SharedInterface.IEcho>();

                //var perfStub = client.Create<SharedInterface.IPerf>();

                //await perfStub.FugaAsync();

                //await perfStub.FooAsync("foo", null, new int[0]);

                //await perfStub.HogeAsync("hoge", 1, SharedInterface.MyEnum.A);

                //await perfStub.VoidTaskAsync();

                ////var result = await stub.EchoAsync(123, 10, SharedInterface.MyEnum.C);

                ////Console.WriteLine(result.ToString());

                //var seq = Enumerable.Range(1, 1000)
                //          .Select(x => stub.EchoAsync("hogehoge", x, 10, SharedInterface.MyEnum.B))
                //          .ToArray();

                //var t = await Task.WhenAll(seq);

                //foreach (var result in t)
                //{
                //    Console.WriteLine(result.Sum);
                //}
            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
            finally
            {
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }
        }
    }
}
