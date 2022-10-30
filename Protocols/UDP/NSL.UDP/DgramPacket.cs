using NSL.SocketCore;
using NSL.SocketCore.Utils.Buffer;

namespace NSL.UDP
{
    public class DgramPacket : OutputPacketBuffer
    {
        public override void Send(IClient client, bool disposeOnSend)
        {
            AppendHash = true;

            base.Send(client, disposeOnSend);
        }
    }
}
