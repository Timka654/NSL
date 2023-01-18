using NSL.UDP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.UDP.Interface
{
    public interface IUDPClient
    {
        void Send(UDPChannelEnum channel, byte[] buffer);
    }
}
