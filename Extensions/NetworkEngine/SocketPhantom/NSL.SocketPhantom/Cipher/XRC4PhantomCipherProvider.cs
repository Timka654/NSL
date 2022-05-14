using NSL.Cipher.RC.RC4;
using SocketCore;

namespace NSL.SocketPhantom.Cipher
{
    public class XRC4PhantomCipherProvider : PhantomCipherProvider
    {
        private string Key;

        public XRC4PhantomCipherProvider(string key)
        {
            Key = key;
        }

        public override void SetProvider(CoreOptions options)
        {
            options.InputCipher = new XRC4Cipher(Key);
            options.OutputCipher = new XRC4Cipher(Key);
        }
    }
}
