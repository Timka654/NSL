using Cipher.RC.RC4;
using SocketCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Cipher
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
            options.inputCipher = new XRC4Cipher(Key);
            options.outputCipher = new XRC4Cipher(Key);
        }
    }
}
