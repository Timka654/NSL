using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.SocketCore
{
    public interface INetworkNode
    {
        void SendEmpty(ushort packetId);

        void Send(byte[] buf);

        void Send(byte[] buf, int offset, int lenght);

        void Send(OutputPacketBuffer packet, bool disposeOnSend = true);

        void Disconnect();
    }
}
