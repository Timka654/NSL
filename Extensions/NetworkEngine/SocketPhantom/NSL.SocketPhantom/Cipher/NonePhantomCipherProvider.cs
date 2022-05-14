using SocketCore;
using SocketCore.Utils.Cipher;

namespace NSL.SocketPhantom.Cipher
{
    public class NonePhantomCipherProvider : PhantomCipherProvider
    {
        public override void SetProvider(CoreOptions options)
        {
            options.InputCipher = new PacketNoneCipher();
            options.OutputCipher = new PacketNoneCipher();
        }
    }
}
