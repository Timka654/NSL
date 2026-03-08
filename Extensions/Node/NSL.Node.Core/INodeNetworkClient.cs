using NSL.UDP;

namespace NSL.Node.Core
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
