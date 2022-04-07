using SocketCore;
using SocketCore.Utils.Cipher;

namespace SocketPhantom.Cipher
{
    public class NonePhantomCipherProvider : PhantomCipherProvider
    {
        public override void SetProvider(CoreOptions options)
        {
            options.inputCipher = new PacketNoneCipher();
            options.outputCipher = new PacketNoneCipher();
        }
    }
}
