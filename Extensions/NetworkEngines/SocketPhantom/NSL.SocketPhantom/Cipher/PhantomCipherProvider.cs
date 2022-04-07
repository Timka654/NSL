using SocketCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Cipher
{
    public abstract class PhantomCipherProvider
    {
        public abstract void SetProvider(CoreOptions options);
    }
}
