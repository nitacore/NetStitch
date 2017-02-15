using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter.Formatters;

namespace ZeroFormatter
{
    public class ZeroFormatterExtensions
    {
        public static object ValueTupleFormatterResolver(Type t)
        {
            var ti = t.GetTypeInfo();

            if (ti.FullName.StartsWith("System.ValueTuple"))
            {
                Type tupleFormatterType = null;
                switch (ti.GetGenericArguments().Length)
                {
                    case 1:
                        tupleFormatterType = typeof(ValueTupleFormatter<,>);
                        break;
                    case 2:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,>);
                        break;
                    case 3:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,>);
                        break;
                    case 4:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,,>);
                        break;
                    case 5:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,,,>);
                        break;
                    case 6:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,,,,>);
                        break;
                    case 7:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,,,,,>);
                        break;
                    case 8:
                        tupleFormatterType = typeof(ValueTupleFormatter<,,,,,,,,>);
                        break;
                    default:
                        break;
                }
                var formatterType = tupleFormatterType.MakeGenericType(ti.GetGenericArguments().StartsWith(typeof(DefaultResolver)));
                return Activator.CreateInstance(formatterType);
            }
            return null; // fallback to the next resolver
        }
    }

    //porting from https://github.com/neuecc/ZeroFormatter/blob/546256b280c1666784deab4768e66582784cc0be/src/ZeroFormatter/Internal/EnumerableExtensions.cs
    internal static class EnumerableExtensions
    {
        public static T[] StartsWith<T>(this T[] array, T firstValue)
        {
            var dest = new T[array.Length + 1];
            dest[0] = firstValue;
            Array.Copy(array, 0, dest, 1, array.Length);
            return dest;
        }
    }
}
