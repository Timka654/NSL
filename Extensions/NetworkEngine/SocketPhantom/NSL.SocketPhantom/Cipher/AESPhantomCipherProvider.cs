using NSL.Cipher;
using NSL.Cipher.AES;
using NSL.SocketCore;

namespace NSL.SocketPhantom.Cipher
{
    public class AESPhantomCipherProvider : PhantomCipherProvider
    {
        private CipherConfiguration Options;

        public AESPhantomCipherProvider(CipherConfiguration options)
        {
            Options = options;
        }

        public override void SetProvider(CoreOptions options)
        {
            var cipher = new AESCipher(Options);

            options.InputCipher = cipher;
            options.OutputCipher = cipher;
        }
    }
}
