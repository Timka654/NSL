using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinarySerializer_v5.Test
{
    public class ITestStruct<T>
    {
        protected BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

        protected byte[] serializerWriteBuffer;

        protected byte[] streamWriteBuffer;

        protected T serializedValue;

        protected T desserializedValue;

        protected T binaryWritedValue;

        protected T binaryReadedValue;

        protected T normalValue;

        public virtual void bsSerializeAction(Stopwatch sw)
        {
            var r = GetRandomValue();

            if (serializedValue == null)
            {
                serializedValue = r;
                serializerWriteBuffer = bs.Serialize(r, "");
            }

            sw.Start();
            bs.Serialize(r, "");
            sw.Stop();
        }

        public virtual void bsDesserializeAction(Stopwatch sw)
        {
            if (desserializedValue == null)
                desserializedValue = bs.Desserialize<T>(serializerWriteBuffer, "");


            sw.Start();
            bs.Desserialize<T>(serializerWriteBuffer, "");
            sw.Stop();
        }

        public virtual void streamWriteFunc(Stopwatch sw)
        {

        }

        public virtual void streamReadFunc(Stopwatch sw)
        {

        }

        public virtual T GetRandomValue()
        {
            return default(T);
        }

        public virtual void Compare()
        {
            bool wbr = CompareWriteBuffers();

            Console.WriteLine($"Compare writed buffers: {wbr}");

            //Console.WriteLine($"Serialize/Write buffer compare: {sbsb}");
        }

        private bool CompareWriteBuffers()
        {
            if (serializerWriteBuffer?.Length != streamWriteBuffer?.Length)
            {
                Console.WriteLine("Invalid length");
                return false;
            }

            for (int i = 0; i < serializerWriteBuffer.Length; i++)
            {
                if (serializerWriteBuffer[i] != streamWriteBuffer[i])
                    return false;
            }

            return true;
        }
    }
}
