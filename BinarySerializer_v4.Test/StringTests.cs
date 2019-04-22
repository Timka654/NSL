using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BinarySerializer.Tests
{
    public partial class Test
    {
        private static MemoryStream StringTestSerialize(TestData.TestStructureString value)
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
                    bs_s.Serialize("", value,1024);
                    s.Stop();
                    Console.WriteLine($"Serialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    buffer = bs_s.Serialize("", value,1024);
                    s.Stop();
                    ms.Write(buffer, 0, bs_s.Length);
                    TimeTable.Add(s.Elapsed.TotalMilliseconds);
                }
                s.Reset();
            }
            Console.WriteLine($"Serialize: Min = ({TimeTable.Min()} ms.) Max = ({TimeTable.Max()} ms.) {Iterations}Avg = ({TimeTable.Average()} ms.) {Iterations}Time = ({TimeTable.Sum()} ms.)");

            
            using (var fs = File.OpenWrite("serializerResults.bin"))
            {
                fs.SetLength(0);
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fs);
            }

            s.Reset();
            TimeTable.Clear();

            BinaryWriter bw = new BinaryWriter(ms2);
            for (int h = 0; h < Iterations; h++)
            {
                ms2.Seek(0, SeekOrigin.Begin);
                if (h == 0)
                {
                    s.Start();
                    bw.Write((Int32)value.len);

                    bw.Write(strToBytes(value.t1, 10));

                    int len = value.len;

                    var valt2 = value.t2;
                    if (value.t2.Length < 55)
                    {
                        Array.Resize(ref valt2, 55);
                    }

                    for (int i = 0; i < 55; i++)
                    {
                        if (valt2[i] == null)
                            valt2[i] = "";
                        bw.Write(strToBytes(valt2[i], len));
                    }
                    bw.Write(strToBytes(value.t3, len));

                    var valt4 = value.t4;
                    if (value.t4.Length < len)
                    {
                        Array.Resize(ref valt4, len);
                    }

                    for (int i = 0; i < len; i++)
                    {
                        if (valt4[i] == null)
                            valt4[i] = "";
                        bw.Write(strToBytes(valt4[i], 10));
                    }

                    var valt5 = value.t5;
                    if (value.t5.Length < 55)
                    {
                        Array.Resize(ref valt5, 55);
                    }

                    for (int i = 0; i < 55; i++)
                    {
                        if (valt5[i] == null)
                            valt5[i] = "";
                        bw.Write(strToBytes(valt5[i], 10));
                        using (var fs = File.OpenWrite("BinaryResults.bin"))
                        {
                            fs.SetLength(0);
                            long coffs = ms2.Position;
                            ms2.Seek(0, SeekOrigin.Begin);
                            ms2.CopyTo(fs);
                            ms2.Seek(coffs, SeekOrigin.Begin);
                        }
                    }
                    s.Stop();
                    Console.WriteLine($"BinaryWriter (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();

                    int len = value.len;

                    bw.Write((Int32)value.len);

                    bw.Write(strToBytes(value.t1, 10));

                    var valt2 = value.t2;
                    if (value.t2.Length < 55)
                    {
                        Array.Resize(ref valt2, 55);
                    }

                    for (int i = 0; i < 55; i++)
                    {
                        if (valt2[i] == null)
                            valt2[i] = "";
                        bw.Write(strToBytes(valt2[i], len));
                    }

                    bw.Write(strToBytes(value.t3, len));

                    var valt4 = value.t4;
                    if (value.t4.Length < len)
                    {
                        Array.Resize(ref valt4, len);
                    }

                    for (int i = 0; i < len; i++)
                    {
                        if (valt4[i] == null)
                            valt4[i] = "";
                        bw.Write(strToBytes(valt4[i], 10));
                    }

                    var valt5 = value.t5;
                    if (value.t5.Length < 55)
                    {
                        Array.Resize(ref valt5, 55);
                    }

                    for (int i = 0; i < 55; i++)
                    {
                        if (valt5[i] == null)
                            valt5[i] = "";
                        bw.Write(strToBytes(valt5[i], 10));
                        
                    }
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
                        Console.WriteLine("Compare Strings: OK");
                    }
                    else
                    {

                        Console.WriteLine("Compare Strings: NO");
                        Console.Write("Serialize Result:");
                        List<string> d = new List<string>();
                        using (var fs = File.OpenWrite("c.bin"))
                        {
                            fs.SetLength(0);
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(fs);
                        }
                        foreach (var item in ms.ToArray())
                        {
                            d.Add("0x" + item.ToString("x2"));
                        }
                        Console.WriteLine(string.Join(" ", d.ToArray()));
                        d.Clear();
                        Console.Write("BinaryWriter Result:");
                        using (var fs = File.OpenWrite("BinaryResults.bin"))
                        {
                            fs.SetLength(0);
                            ms2.Seek(0, SeekOrigin.Begin);
                            ms2.CopyTo(fs);
                        }
                        foreach (var item in ms2.ToArray())
                        {
                            d.Add("0x" + item.ToString("x2"));
                        }
                        Console.WriteLine(string.Join(" ", d.ToArray()));
                    }
                }
            return ms;
        }

        private static byte[] strToBytes(string text, int len)
        {
            byte[] r = new byte[len];
            int br = Encoding.UTF8.GetByteCount(text);
            Array.Copy(Encoding.UTF8.GetBytes(text), r, br < len? br:len);
            return r;
        }

        private static string bytesToStr(byte[] data)
        {
            return Encoding.UTF8.GetString(data).Replace("\0","");
        }

        private static void StringTestDeserialize(MemoryStream ms, TestData.TestStructureString value)
        {
            ms.Seek(0, SeekOrigin.Begin);

            TestData.TestStructureString val = null;

            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();

            BinarySerializer bs_s = new BinarySerializer();

            List<double> TimeTable = new List<double>();
            for (int i = 0; i < Iterations; i++)
            {
                ms.Seek(0, SeekOrigin.Begin);
                if (i == 0)
                {
                    s.Start();
                    bs_s.Deserialize<TestData.TestStructureString>("", ms.GetBuffer());
                    s.Stop();
                    Console.WriteLine($"Deserialize (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    val = bs_s.Deserialize<TestData.TestStructureString>("", ms.GetBuffer());
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
                    value.len = br.ReadInt32();
                    //bw.Write((Int32)value.len);
                    value.t1 = bytesToStr(br.ReadBytes(10));
                    //bw.Write(strToBytes(value.t1, 10));
                    
                    int len = value.len;

                    value.t2 = new string[55];
                    //var valt2 = value.t2;
                    //if (value.t2.Length < 55)
                    //{
                    //    Array.Resize(ref valt2, 55);
                    //}

                    for (int i = 0; i < 55; i++)
                    {
                        value.t2[i] = bytesToStr(br.ReadBytes(len));
                        //if (valt2[i] == null)
                        //    valt2[i] = new string();
                        //bw.Write(strToBytes(valt2[i], len));
                    }
                    value.t3 = bytesToStr(br.ReadBytes(len));
                    //bw.Write(strToBytes(value.t3, len));

                    value.t4 = new string[len];
                    //var valt4 = value.t4;
                    //if (value.t4.Length < len)
                    //{
                    //    Array.Resize(ref valt4, len);
                    //}

                    for (int i = 0; i < len; i++)
                    {
                        value.t4[i] = bytesToStr(br.ReadBytes(10));
                        //if (valt4[i] == null)
                        //    valt4[i] = new string();
                        //bw.Write(strToBytes(valt4[i], 10));
                    }

                    value.t5 = new string[55];
                    //var valt5 = value.t5;
                    //if (value.t5.Length < 55)
                    //{
                    //    Array.Resize(ref valt5, 55);
                    //}

                    for (int i = 0; i < 55; i++)
                    {
                        value.t5[i] = bytesToStr(br.ReadBytes(10));
                        //if (valt5[i] == null)
                        //    valt5[i] = new string();
                        //bw.Write(strToBytes(valt5[i], 10));
                    }
                    s.Stop();
                    Console.WriteLine($"BinaryReader (first): {s.Elapsed.ToString()} ms.");
                }
                else
                {
                    s.Start();
                    value.len = br.ReadInt32();
                    //bw.Write((Int32)value.len);
                    value.t1 = bytesToStr(br.ReadBytes(10));
                    //bw.Write(strToBytes(value.t1, 10));

                    int len = value.len;

                    value.t2 = new string[55];
                    //var valt2 = value.t2;
                    //if (value.t2.Length < 55)
                    //{
                    //    Array.Resize(ref valt2, 55);
                    //}

                    for (int i = 0; i < 55; i++)
                    {
                        value.t2[i] = bytesToStr(br.ReadBytes(len));
                        //if (valt2[i] == null)
                        //    valt2[i] = new string();
                        //bw.Write(strToBytes(valt2[i], len));
                    }
                    value.t3 = bytesToStr(br.ReadBytes(len));
                    //bw.Write(strToBytes(value.t3, len));

                    value.t4 = new string[len];
                    //var valt4 = value.t4;
                    //if (value.t4.Length < len)
                    //{
                    //    Array.Resize(ref valt4, len);
                    //}

                    for (int i = 0; i < len; i++)
                    {
                        value.t4[i] = bytesToStr(br.ReadBytes(10));
                        //if (valt4[i] == null)
                        //    valt4[i] = new string();
                        //bw.Write(strToBytes(valt4[i], 10));
                    }

                    value.t5 = new string[55];
                    //var valt5 = value.t5;
                    //if (value.t5.Length < 55)
                    //{
                    //    Array.Resize(ref valt5, 55);
                    //}

                    for (int i = 0; i < 55; i++)
                    {
                        value.t5[i] = bytesToStr(br.ReadBytes(10));
                        //if (valt5[i] == null)
                        //    valt5[i] = new string();
                        //bw.Write(strToBytes(valt5[i], 10));
                    }
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
                Cmp(typeof(TestData.TestStructureString), value, val);
            }
        }
        public static void StringTest(TestData.TestStructureString value)
        {
            Console.WriteLine("String Test:");
            StringTestDeserialize(StringTestSerialize(value), value);
        }
    }
}
