using NSL.SocketClient;
using NSL.SocketCore.Extensions.Buffer;

namespace NSL.Node.RoomServer.Bridge
{
    public class BridgeRoomNetworkClient : BaseSocketNetworkClient
    {
        public RequestProcessor PacketWaitBuffer { get; }

        public BridgeRoomNetworkClient()
        {
            PacketWaitBuffer = new RequestProcessor(this);
        }

        public override void Dispose()
        {
            PacketWaitBuffer.Dispose();

            base.Dispose();
        }
    }
}
