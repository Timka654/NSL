using NSL.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public interface INodeNetworkClient
    {
        int SendBytesRate { get; }
        int ReceiveBytesRate { get; }
        int MINPing { get; }
        int MAXPing { get; }
        int AVGPing { get; }

        void Disconnect();
        void Send(DgramOutputPacketBuffer buffer, bool disposeOnSend = true);
    }
}
