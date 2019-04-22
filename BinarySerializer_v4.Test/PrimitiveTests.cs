using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BinarySerializer.Tests
{
    public partial class Test
    {
        private static MemoryStream PrimitiveTestSerialize(TestData.TestStructureNumeric value)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            MemoryStream ms = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();
            List<double> TimeTable = new List<double>();
                BinarySerializer bs_s = new BinarySerializer()
                {
                    //AutoResizeArrayClasses = true,
                    //AutoResizeArrayPrimitives = true,
                    //AutoInitializeNullClasses = true,
                    //AutoInitializeNullPrimitives = true
                };
            byte[] buffer;
            for (int i = 0; i < Iterations; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (i == 0)
                {
                    s.Start();
                    bs_s.Serialize("", value, 1024);
                    s.Stop();
                    Console.WriteLine($"Serialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    buffer = bs_s.Serialize("", value, 1024);
                    s.Stop();
                    ms.Write(buffer, 0, bs_s.Length);
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"Serialize: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");


            s.Reset();
            TimeTable.Clear();

            BinaryWriter bw = new BinaryWriter(ms2);
            for (int h = 0; h < Iterations; h++)
            {
                ms2.Seek(0, SeekOrigin.Begin);
                if (h == 0)
                {
                    s.Start();
                    bw.Write((Int16)value.t1);
                    bw.Write((Int32)value.t2);
                    bw.Write((Int64)value.t3);

                    bw.Write((float)value.t4);
                    bw.Write((double)value.t5);
                    bw.Write((byte)value.t6);
                    s.Stop();
                    Console.WriteLine($"BinaryWriter (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    bw.Write((Int16)value.t1);
                    bw.Write((Int32)value.t2);
                    bw.Write((Int64)value.t3);

                    bw.Write((float)value.t4);
                    bw.Write((double)value.t5);
                    bw.Write((byte)value.t6);
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"BinaryWriter: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");

            if (ms.Length != ms2.Length)
                Console.WriteLine($"Serialize: invalid lenght({ms.Length} vs {ms2.Length}).");
            else
            {
                if (ms.ToArray().SequenceEqual(ms2.ToArray()))
                {
                    Console.WriteLine("Compare Primitive: OK");
                }
                else
                {
                    Console.WriteLine("Compare Primitive: NO");
                    Console.Write("Serialize Result:");
                    List<string> d = new List<string>();
                    foreach (var item in ms.ToArray())
                    {
                        d.Add("0x" + item.ToString("x2"));
                    }
                    Console.WriteLine(string.Join(" ", d.ToArray()));
                    d.Clear();
                    Console.Write("BinaryWriter Result:");
                    foreach (var item in ms2.ToArray())
                    {
                        d.Add("0x" + item.ToString("x2"));
                    }
                    Console.WriteLine(string.Join(" ", d.ToArray()));
                }
            }

            return ms;
        }

        private static void PrimitiveTestDeserialize(MemoryStream ms, TestData.TestStructureNumeric value)
        {
            ms.Seek(0, SeekOrigin.Begin);

            TestData.TestStructureNumeric val = null;

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

            BinarySerializer bs_s = new BinarySerializer();
            List<double> TimeTable = new List<double>();
            for (int i = 0; i < Iterations; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (i == 0)
                {
                    s.Start();
                    bs_s.Deserialize<TestData.TestStructureNumeric>("", ms.GetBuffer());
                    s.Stop();
                    Console.WriteLine($"Deserialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    val = bs_s.Deserialize<TestData.TestStructureNumeric>("", ms.GetBuffer());
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"Deserialize: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");


            s.Reset();
            TimeTable.Clear();

            BinaryReader br = new BinaryReader(ms);
            for (int h = 0; h < Iterations; h++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (h == 0)
                {
                    s.Start();
                    value.t1 = br.ReadInt16();
                    value.t2 = br.ReadInt32();
                    value.t3 = br.ReadInt64();
                    value.t4 = br.ReadSingle();
                    value.t5 = br.ReadDouble();
                    value.t6 = br.ReadByte();
                    s.Stop();
                    Console.WriteLine($"BinaryReader (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    value.t1 = br.ReadInt16();
                    value.t2 = br.ReadInt32();
                    value.t3 = br.ReadInt64();
                    value.t4 = br.ReadSingle();
                    value.t5 = br.ReadDouble();
                    value.t6 = br.ReadByte();
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"BinaryReader: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");

            if (Compare)
            {
                Console.WriteLine();
                Console.WriteLine($"Compare Results:");
                Console.WriteLine($"(Name) : (Source) = (Dest)");
                Cmp(typeof(TestData.TestStructureNumeric), value, val);
            }
        }

        public static void PrimitiveTest(TestData.TestStructureNumeric value)
        {
            Console.WriteLine("Primitive Test:");
            PrimitiveTestDeserialize(PrimitiveTestSerialize(value), value);
        }
    }
}
