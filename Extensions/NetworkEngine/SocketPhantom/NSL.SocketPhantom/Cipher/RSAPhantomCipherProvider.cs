using NSL.Cipher.RSA;
using NSL.SocketCore;

namespace NSL.SocketPhantom.Cipher
{
    public class RSAPhantomCipherProvider : PhantomCipherProvider
    {
        private string XmlKey;

        public RSAPhantomCipherProvider(string xmlKey)
        {
            XmlKey = xmlKey;
        }

        public override void SetProvider(CoreOptions options)
        {
            var cipher = new RSACipher();
            cipher.LoadXml(XmlKey);

            options.InputCipher = cipher;
            options.OutputCipher = cipher;
        }
    }
}
