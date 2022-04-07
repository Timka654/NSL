using Cipher;
using Cipher.AES;
using SocketCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Cipher
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

            options.inputCipher = cipher;
            options.outputCipher = cipher;
        }
    }
}
