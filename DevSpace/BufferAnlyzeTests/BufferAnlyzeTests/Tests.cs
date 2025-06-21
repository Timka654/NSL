using BufferAnlyzeTests.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BufferAnlyzeTests
{
    public class Tests
    {
        private List<double> Test(string testName, Action<Stopwatch> action, int time = 10000)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            action.Invoke(sw);
            sw.Stop();

            Console.WriteLine($"Test \"{testName}\" first elapsed: {sw.Elapsed.TotalMilliseconds} ms.");

            List<double> result = new List<double>();

            for (int i = 0; i < time; i++)
            {
                sw.Reset();
                action.Invoke(sw);
                sw.Stop();
                result.Add(sw.Elapsed.TotalMilliseconds);
            }

            Console.WriteLine($"Test results from \"{testName}\" in {time} elapsed min = {result.Min()} ms.  avg = {result.Average()} ms.  max = {result.Max()} ms.");

            return result;
        }

        public void Run()
        {

            int i = 9999;
            Test("noExep", (sw) =>
            {
                if (i > 255)
                {

                }
                else
                { 
                
                }

            });


            //InputTests();

            Console.ReadKey();
        }

        private void InputTests()
        {
            var nodeResult = Test("NodeInputBuffer", (sw) =>
            {
                NodeOutputPacketBuffer buf = new NodeOutputPacketBuffer();

                WriteData(buf);
                byte[] bf = buf.Finalize();

                sw.Start();
                NodeInputPacketBuffer rbuf = new NodeInputPacketBuffer(bf,0);

                ReadData(rbuf);
            });

            //var tempResult = Test("TempInputBuffer", (sw) =>
            //{
            //    sw.Start();
            //    TempInputPacketBuffer buf = new TempInputPacketBuffer();

            //    WriteData(buf);
            //    buf.Finalize();
            //});

            var memoryResult = Test("MemoryInputBuffer", (sw) =>
            {
                MemoryOutputPacketBuffer buf = new MemoryOutputPacketBuffer(100);
                buf.PacketId = 49;

                WriteData(buf);
                var bf = buf.Finalize();

                sw.Start();

                MemoryInputPacketBuffer rbuf = new MemoryInputPacketBuffer(bf, 0);

                ReadData(rbuf);
            });
        }

        private void OutputTests()
        {
            var nodeResult = Test("NodeOutputBuffer", (sw) =>
            {
                sw.Start();
                NodeOutputPacketBuffer buf = new NodeOutputPacketBuffer();

                WriteData(buf);
                buf.Finalize();
            });

            var nodeFixedResult = Test("NodeOutputBuffer fixed size", (sw) =>
            {
                sw.Start();
                NodeOutputPacketBuffer buf = new NodeOutputPacketBuffer(100);

                WriteData(buf);
                buf.Finalize();
            });

            var tempResult = Test("TempOutputBuffer", (sw) =>
            {
                sw.Start();
                TempOutputPacketBuffer buf = new TempOutputPacketBuffer();

                WriteData(buf);
                buf.Finalize();
            });

            var memoryResult = Test("MemoryOutputBuffer", (sw) =>
            {
                sw.Start();
                MemoryOutputPacketBuffer buf = new MemoryOutputPacketBuffer();

                WriteData(buf);
                buf.Finalize();
            });

            var memoryFixedResult = Test("MemoryOutputBuffer fixed size", (sw) =>
            {
                sw.Start();
                MemoryOutputPacketBuffer buf = new MemoryOutputPacketBuffer(100);
                buf.PacketId = 49;

                WriteData(buf);
                buf.Finalize();
            });
        }

        private void ReadData(IReadBuffer buf)
        {
            buf.ReadByte();
            buf.ReadFloat32();
            buf.ReadFloat64();
            buf.ReadInt16();
            buf.ReadInt32();
            buf.ReadInt64();
            buf.ReadUInt16();
            buf.ReadUInt32();
            buf.ReadUInt64(); 
            
            buf.ReadFloat32();
            buf.ReadFloat64();
            buf.ReadInt16();
            buf.ReadInt32();
            buf.ReadInt64();
            buf.ReadUInt16();
            buf.ReadUInt32();
            buf.ReadUInt64();

        }

        private void WriteData(IWriteBuffer buf)
        {
            buf.WriteByte(53);
            buf.WriteFloat32(5423f);
            buf.WriteFloat64(6542);
            buf.WriteInt16(2344);
            buf.WriteInt32(3432);
            buf.WriteInt64(325633);
            buf.WriteUInt16(1111);
            buf.WriteUInt32(7645);
            buf.WriteUInt64(34323523556);
            
            buf.WriteFloat32(5423f);
            buf.WriteFloat64(6542);
            buf.WriteInt16(2344);
            buf.WriteInt32(3432);
            buf.WriteInt64(325633);
            buf.WriteUInt16(1111);
            buf.WriteUInt32(7645);
            buf.WriteUInt64(34323523556);
        }


    }
}
