using NSL.SocketCore;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NSL.EndPointBuilder
{
    public interface IHandleIOBuilder {
        void AddBaseReceiveHandle(ReceivePacketDebugInfo<IClient> handle);

        void AddBaseSendHandle(SendPacketDebugInfo<IClient> handle);
    }

    public interface IHandleIOBuilder<TClient> : IHandleIOBuilder
        where TClient : IClient
    {
        void AddReceiveHandle(SocketCore.ReceivePacketDebugInfo<TClient> handle);

        void AddSendHandle(SocketCore.SendPacketDebugInfo<TClient> handle);
    }
}
