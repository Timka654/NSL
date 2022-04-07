using SocketCore;
using SocketCore.Utils.Cipher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
