using NSL.Cipher.RC.RC4;

namespace Cipher.TestExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestXRC4();
            Console.WriteLine("Hello, World!");
        }
        static void TestXRC4()
        {
            var enc = new XRC4Cipher("ath3t2ty677545t34r3e");
            var dec = new XRC4Cipher("ath3t2ty677545t34r3e");


            var d = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            enc.EncodeHeaderRef(ref d, 0);
            enc.EncodeRef(ref d, 7, 3);

            dec.DecodeHeaderRef(ref d, 0);

            dec.DecodeRef(ref d, 7, 3);

        }
    }
}
