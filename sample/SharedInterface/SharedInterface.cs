using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharedInterface;
using NetStitch;
using System.Linq;
using ZeroFormatter;

namespace SharedInterface
{
    [NetStitchContract]
    public interface IComplexType
    {
        [Operation("83576334-4631-44df-a63b-f43a53528551")]
#if !___server___
        Task<
#endif

        MyClass
#if !___server___
>
#endif
#if !___server___
EchoAsync
#else
Echo
#endif
(MyClass myClass
#if !___server___
, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
);

    }

    [NetStitchContract]
    public interface IEcho
    {

        [Operation("548aac9b-e7c3-4b78-a4d7-0bbfa0a8a2f4")]
#if !___server___
        Task<
#endif
        int
#if !___server___
>
#endif
#if !___server___
SumAsync
#else
Sum
#endif
(int[] array
#if !___server___
, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
);

        [Operation("5000")]
#if !___server___
        Task<
#endif
        MyClass
#if !___server___
        >
#endif
#if !___server___
          EchoAsync
#else
          Echo
#endif
        ( string name, int x, int y, MyEnum e
#if !___server___
            , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );
    }

    [NetStitchContract]
    public interface IPerf
    {
        // MyClass Foo(string a, int? x, int[] array);
        [Operation("fuga")]
#if !___server___
        Task<
#endif
        MyClass
#if !___server___
        >
#endif

#if !___server___
          FooAsync(
#else
          Foo(
#endif
            string a, int? x, int[] array
#if !___server___
            , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif

        );

        //Task<int> HogeAsync(string a, int x, MyEnum e);
        [Operation("hoge")]
        Task<int> HogeAsync(string a, int x, MyEnum e = MyEnum.B
#if !___server___
            , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        //void Fuga();
        [Operation("3")]
#if !___server___
        Task
#else
        void
#endif

#if !___server___
          FugaAsync(
#else
          Fuga(
#endif

#if !___server___
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        //Task VoidTask();
        [Operation("4")]
        Task VoidTaskAsync(
#if !___server___
            System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
            );

    }

    [ZeroFormattable]
    public class MyClass
    {
        [Index(0)]
        public virtual string Name { get; set; }
        [Index(1)]
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
