using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;
using System.Collections.Concurrent;

namespace NSL.Node.BridgeServer.LS
{
    internal class LobbyServerNetworkClient : IServerNetworkClient
    {
        public string Identity { get; set; }


        public PacketWaitBuffer ValidateRequestBuffer { get; set; }

        public LobbyServerNetworkClient() : base()
        {
            ValidateRequestBuffer = new PacketWaitBuffer(this);
        }

        public override void Dispose()
        {
            if (ValidateRequestBuffer != null)
                ValidateRequestBuffer.Dispose();

            base.Dispose();
        }

    }
}
