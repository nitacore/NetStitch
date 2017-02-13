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
        [NetStitch.Operation("a885e7c2-4305-4d2c-8428-9936125641a7")]
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
        [NetStitch.Operation("d1ae3ebb-1743-46d7-acc7-1f4afc0f7592")]
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
        [NetStitch.Operation("22b39868-64d6-4ac5-944e-d30d937c3e18")]
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
        (string name, int x, int y, MyEnum e
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

    }

    [NetStitchContract]
    public interface IPerf
    {
        [NetStitch.Operation("6afa8955-4f73-4a45-8795-b980763a7182")]
#if !___server___
        Task<
#endif
        MyClass
#if !___server___
        >
#endif
#if !___server___
        FooAsync
#else
        Foo
#endif
        (string a, int? x, int[] array
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        [NetStitch.Operation("eb28bf13-ce0b-41ee-a4dc-28c456687707")]
        Task<int> HogeAsync(string a, int x, MyEnum e
#if !___server___
        , System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        [NetStitch.Operation("019ec065-10b2-43ed-85a2-57b27aab9762")]
#if !___server___
        Task
#else
        void
#endif
#if !___server___
        FugaAsync
#else
        Fuga
#endif
        (
#if !___server___
        System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)
#endif
        );

        [NetStitch.Operation("bb1934e2-f82e-41d3-845d-3bface2dae8d")]
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
