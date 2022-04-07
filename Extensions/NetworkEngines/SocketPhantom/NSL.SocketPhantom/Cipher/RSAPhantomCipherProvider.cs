using Cipher.RSA;
using SocketCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Cipher
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

            options.inputCipher = cipher;
            options.outputCipher = cipher;
        }
    }
}
