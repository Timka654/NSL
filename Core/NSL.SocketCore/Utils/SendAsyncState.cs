using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Utils
{
    public class SendAsyncState
    {
        public byte[] buf { get; set; }

        public int offset { get; set; }

        public int len { get; set; }
    }
}
