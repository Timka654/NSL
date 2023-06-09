using NSL.SocketCore.Utils.Buffer;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{
	class Program
	{
		static void Main(string[] args)
		{
			var o = new OutputPacketBuffer();

			 new TestStruct1() { s1 = " abbbbcc", n1 = 75656 }.BinaryWriteFull(o);

			var i = new InputPacketBuffer(o.CompilePacket());

			var s = TestStruct1.BinaryReadFull(i);
        }
	}
}