using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharedInterface;
using NetStitch;
using System.Linq;
using MessagePack;

namespace SharedInterface
{
    public interface IComplexType : INetStitchContract
    {
        [Operation]
        ValueTask<MyClass> EchoAsync(MyClass myClass
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }

    public interface IAsyncTest : INetStitchContract
    {
        ValueTask<int> TestAsync(int a, int b
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }

    public interface IEcho : INetStitchContract
    {
        ValueTask<int> SumAsync(int[] array
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
        ValueTask<MyClass> EchoAsync(string name, int x, int y, MyEnum e
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

    }

    public interface IPerf : INetStitchContract
    {
        ValueTask<MyClass> FooAsync(string a, int? x, int[] array
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        ValueTask<int> HogeAsync(string a, int x, MyEnum e
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        Task FugaAsync(
#if !___server___
        System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        Task VoidTaskAsync(
#if !___server___
        System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

    }

    [MessagePackObject]
    public class MyClass
    {
        [Key(0)]
        public virtual string Name { get; set; }
        [Key(1)]
        public virtual int Sum { get; set; }
    }

    public enum MyEnum
    {
        A = 2,
        B = 3,
        C = 4
    }

    public enum MyEnum2 : ulong
    {
        A = 100,
        B = 3000,
        C = 50000
    }
}
