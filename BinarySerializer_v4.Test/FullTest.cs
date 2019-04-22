using BinarySerializer.DefaultTypes;
using BinarySerializer.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BinarySerializer.Tests
{
    public partial class Test
    {
        private static List<Type> ptypes = new List<Type>()
        {
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(string),
            typeof(byte[]),
            typeof(short[]),
            typeof(ushort[]),
            typeof(int[]),
            typeof(uint[]),
            typeof(long[]),
            typeof(ulong[]),
            typeof(float[]),
            typeof(double[]),
            typeof(string[]),
        };

        private static void Cmp(Type type, object v1, object v2)
        {
            var props = type.GetProperties();
            foreach (var item in props)
            {
                var attrbs = item.GetCustomAttributes(typeof(BinaryAttribute), false);
                BinaryAttribute battr = attrbs.Length == 0 ? null : (BinaryAttribute)attrbs[0];

                if (battr == null)
                    continue;

                if (item.PropertyType.IsArray)
                {
                    if (ptypes.Contains(item.PropertyType))
                    {
                        Array o1 = (Array)item.GetValue(v1, null);
                        Array o2 = (Array)item.GetValue(v2, null);
                        Console.WriteLine($"{item.Name} count : {o1.Length} = {o2.Length}");
                        for (int i = 0; i < o1.Length && i < o2.Length; i++)
                        {
                            Console.WriteLine($"[{i}] : {o1.GetValue(i)} = {o2.GetValue(i)}");
                        }
                    }
                    else
                    {
                        var o1 = (Array)item.GetValue(v1, null);
                        var o2 = (Array)item.GetValue(v2, null);
                        Console.WriteLine($"{item.Name} count : {o1.Length} = {o2.Length}");
                        for (int i = 0; i < o1.Length && i < o2.Length; i++)
                        {
                            Console.WriteLine($"{item.Name} [{i}] :");


                               Cmp(battr.Type, o1.GetValue(i), o2.GetValue(i));
                        }
                    }
                }
                else
                {
                    if (ptypes.Contains(item.PropertyType))
                    {
                        var o1 = item.GetValue(v1, null);
                        var o2 = item.GetValue(v2, null);
                        Console.WriteLine($"{item.Name} : {o1} = {o2}");
                    }
                    else
                    {
                        var o1 = item.GetValue(v1, null);
                        var o2 = item.GetValue(v2, null);
                        Console.WriteLine($"{item.Name} : ");
                        Cmp(battr.Type, o1, o2);
                    }
                }
            }
        }

        public static bool Compare { get; set; }

        public static int Iterations { get; set; }

        public static void FullTest(TestData.TestStructure value)
        {  
            List<double> TimeTable = new List<double>();

            Stopwatch s = new Stopwatch();
            for (int i = 0; i < Iterations; i++)
            {
                if (i == 0)
                {
                    s.Start();
                    TypeHelper.FillDynamicProperties(value.t1);
                    TypeHelper.FillDynamicProperties(value.t2);
                    TypeHelper.FillDynamicProperties(value.t3);
                    s.Stop();
                    Console.WriteLine($"Fill (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    TypeHelper.FillDynamicProperties(value.t1);
                    TypeHelper.FillDynamicProperties(value.t2);
                    TypeHelper.FillDynamicProperties(value.t3);
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"Fill: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");
            PrimitiveTest(value.t1);
            Console.WriteLine();
            Console.WriteLine();
            ArrayTest(value.t2);
            Console.WriteLine();
            Console.WriteLine();
            StringTest(value.t3);
        }
    }
}
