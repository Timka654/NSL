using NSL.SocketCore.Utils.Buffer;

namespace NSL.Generators.BinaryTypeIOGenerator.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var o = new OutputPacketBuffer();

            new TestStruct1() { s1 = " abbbbcc", n1 = 75656 }.WriteabcTo(o);

            var i = new InputPacketBuffer(o.CompilePacket());

            var s = TestStruct1.ReadabcFrom(i);





            var o1 = new OutputPacketBuffer();

            new TestStruct1() { s1 = "rwegrewgwrg", n1 = 6347 }.WriteenTo(o1);

            var i1 = new InputPacketBuffer(o1.CompilePacket());

            var s1 = TestStruct1.ReadenFrom(i1);

        }
	}
}