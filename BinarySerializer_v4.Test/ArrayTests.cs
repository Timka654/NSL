using BinarySerializer;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace BinarySerializer.Tests
{
    public partial class Test
    {
        private static MemoryStream ArrayTestSerialize(TestData.TestStructureArray value)
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            MemoryStream ms = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();
            List<double> TimeTable = new List<double>();
            BinarySerializer bs_s = new BinarySerializer();
            byte[] buffer;
            for (int i = 0; i < Iterations; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (i == 0)
                {
                    s.Start();
                    bs_s.Serialize("",value, 1024);
                    s.Stop();
                    Console.WriteLine($"Serialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    buffer = bs_s.Serialize("", value, 1024);
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                    ms.Write(buffer,0, bs_s.Length);
                }
                s.Reset();
            }
            Console.WriteLine($"Serialize: Min = ({TimeTable.Min(x=>x)} ms.) Max = ({TimeTable.Max(x=>x)} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum(x=>x)} ms.)");


            s.Reset();
            TimeTable.Clear();

            BinaryWriter bw = new BinaryWriter(ms2);

            for (int h = 0; h < Iterations; h++)
            {
                ms2.Seek(0, SeekOrigin.Begin);
                if (h == 0)
                {
                    s.Start();
                    bw.Write((int)value.len);

                    for (int i = 0; i < 10; i++)
                    {
                        if (value.t1.Length > i)
                        {
                            bw.Write((float)value.t1[i]);
                        }
                        else
                        {
                            bw.Write((float)0);
                        }

                    }

                    for (int i = 0; i < value.len; i++)
                    {
                        if (value.t2.Length > i)
                        {
                            bw.Write((float)value.t2[i]);
                        }
                        else
                        {
                            bw.Write((float)0);
                        }

                    }

                    for (int i = 0; i < 10; i++)
                    {
                        if (value.t3.Length > i)
                        {
                            bw.Write((long)value.t3[i]);
                        }
                        else
                        {
                            bw.Write((long)0);
                        }

                    }

                    for (int i = 0; i < value.len; i++)
                    {
                        if (value.t4.Length > i)
                        {
                            bw.Write((long)value.t4[i]);
                        }
                        else
                        {
                            bw.Write((long)0);
                        }

                    }
                    for (int i = 0; i < value.len; i++)
                    {
                        if (value._t7.Length > i)
                        {
                            var v = value._t7[i];

                            bw.Write((Int16)v.t1);
                            bw.Write((Int32)v.t2);
                            bw.Write((Int64)v.t3);

                            bw.Write((float)v.t4);
                            bw.Write((double)v.t5);
                            bw.Write((byte)v.t6);
                        }
                        else
                        {
                            bw.Write((Int16)0);
                            bw.Write((Int32)0);
                            bw.Write((Int64)0);

                            bw.Write((float)0);
                            bw.Write((double)0);
                            bw.Write((byte)0);
                        }

                    }
                    s.Stop();
                    Console.WriteLine($"BinaryWriter (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    bw.Write((int)value.len);

                    for (int i = 0; i < 10; i++)
                    {
                        if (value.t1.Length > i)
                        {
                            bw.Write((float)value.t1[i]);
                        }
                        else
                        {
                            bw.Write((float)0);
                        }

                    }

                    for (int i = 0; i < value.len; i++)
                    {
                        if (value.t2.Length > i)
                        {
                            bw.Write((float)value.t2[i]);
                        }
                        else
                        {
                            bw.Write((float)0);
                        }

                    }

                    for (int i = 0; i < 10; i++)
                    {
                        if (value.t3.Length > i)
                        {
                            bw.Write((long)value.t3[i]);
                        }
                        else
                        {
                            bw.Write((long)0);
                        }

                    }

                    for (int i = 0; i < value.len; i++)
                    {
                        if (value.t4.Length > i)
                        {
                            bw.Write((long)value.t4[i]);
                        }
                        else
                        {
                            bw.Write((long)0);
                        }

                    }
                    for (int i = 0; i < value.len; i++)
                    {
                        if (value._t7.Length > i)
                        {
                            var v = value._t7[i];

                            bw.Write((Int16)v.t1);
                            bw.Write((Int32)v.t2);
                            bw.Write((Int64)v.t3);

                            bw.Write((float)v.t4);
                            bw.Write((double)v.t5);
                            bw.Write((byte)v.t6);
                        }
                        else
                        {
                            bw.Write((Int16)0);
                            bw.Write((Int32)0);
                            bw.Write((Int64)0);

                            bw.Write((float)0);
                            bw.Write((double)0);
                            bw.Write((byte)0);
                        }

                    }
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"BinaryWriter: Min = ({TimeTable.Min(x=>x)} ms.) Max = ({TimeTable.Max(x=>x)} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum(x=>x)} ms.)");

            if (ms.Length != ms2.Length)
                Console.WriteLine($"Serialize: invalid lenght({ms.Length} vs {ms2.Length}).");
            else
            {
                if (ms.ToArray().SequenceEqual(ms2.ToArray()))
                {
                    Console.WriteLine("Compare Array: OK");
                }
                else
                {

                    Console.WriteLine("Compare Array: NO");
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

        private static void ArrayTestDeserialize(MemoryStream ms, TestData.TestStructureArray value)
        {
            ms.Seek(0, SeekOrigin.Begin);

            TestData.TestStructureArray val = null;

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

            BinarySerializer bs_s = new BinarySerializer();

            List<double> TimeTable = new List<double>();

            for (int i = 0; i < Iterations; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (i == 0)
                {
                    s.Start();
                    bs_s.Deserialize<TestData.TestStructureArray>("", ms.GetBuffer());
                    s.Stop();
                    Console.WriteLine($"Deserialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    val = bs_s.Deserialize<TestData.TestStructureArray>("", ms.GetBuffer());
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"Deserialize: Min = ({TimeTable.Min(x=>x)} ms.) Max = ({TimeTable.Max(x=>x)} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum(x=>x)} ms.)");


            s.Reset();
            TimeTable.Clear();

            BinaryReader br = new BinaryReader(ms);
            for (int h = 0; h < Iterations; h++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (h == 0)
                {
                    s.Start();
                    value.len = br.ReadInt32();

                    value.t1 = new float[10];
                    for (int i = 0; i < 10; i++)
                    {
                        value.t1[i] = br.ReadSingle();
                    }

                    value.t2 = new float[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value.t2[i] = br.ReadSingle();
                    }

                    value.t3 = new long[10];
                    for (int i = 0; i < 10; i++)
                    {
                        value.t3[i] = br.ReadInt64();

                    }

                    value.t4 = new long[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value.t4[i] = br.ReadInt64();
                    }

                    value._t7 = new TestData.TestStructureNumeric[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value._t7[i] = new TestData.TestStructureNumeric();
                        value._t7[i].t1 = br.ReadInt16();
                        value._t7[i].t2 = br.ReadInt32();
                        value._t7[i].t3 = br.ReadInt64();
                        value._t7[i].t4 = br.ReadSingle();
                        value._t7[i].t5 = br.ReadDouble();
                        value._t7[i].t6 = br.ReadByte();
                    }
                    s.Stop();
                    Console.WriteLine($"BinaryReader (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    value.len = br.ReadInt32();

                    value.t1 = new float[10];
                    for (int i = 0; i < 10; i++)
                    {
                        value.t1[i] = br.ReadSingle();
                    }

                    value.t2 = new float[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value.t2[i] = br.ReadSingle();
                    }

                    value.t3 = new long[10];
                    for (int i = 0; i < 10; i++)
                    {
                        value.t3[i] = br.ReadInt64();

                    }

                    value.t4 = new long[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value.t4[i] = br.ReadInt64();
                    }

                    value._t7 = new TestData.TestStructureNumeric[value.len];
                    for (int i = 0; i < value.len; i++)
                    {
                        value._t7[i] = new TestData.TestStructureNumeric();
                        value._t7[i].t1 = br.ReadInt16();
                        value._t7[i].t2 = br.ReadInt32();
                        value._t7[i].t3 = br.ReadInt64();
                        value._t7[i].t4 = br.ReadSingle();
                        value._t7[i].t5 = br.ReadDouble();
                        value._t7[i].t6 = br.ReadByte();
                    }
                    s.Stop();
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"BinaryReader: Min = ({TimeTable.Min(x=>x)} ms.) Max = ({TimeTable.Max(x=>x)} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum(x=>x)} ms.)");

            if (Compare)
            {
                Console.WriteLine();
                Console.WriteLine($"Compare Results:");
                Console.WriteLine($"(Name) : (Source) = (Dest)");
                Cmp(typeof(TestData.TestStructureArray), value, val);
            }
        }

        public static void ArrayTest(TestData.TestStructureArray value)
        {
            Console.WriteLine("Array Test:");
            ArrayTestDeserialize(ArrayTestSerialize(value), value);
        }
    }
}