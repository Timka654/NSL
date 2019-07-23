using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCL.SocketClient.Utils
{
    public interface ILockedPacket
    {
        void UnlockPacket();
    }
}
