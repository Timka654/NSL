using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer.Utils.SystemPackets;
using System.Threading.Tasks;

namespace SocketServer.Utils
{
    public class IServerNetworkClient : INetworkClient
    {
        public CoreOptions ServerOptions { get; set; }

        public async void RunAliveChecker()
        {
            await Task.Delay(AliveCheckTimeOut);
            do
            {
                AliveConnection<IServerNetworkClient>.Send(this);
                await Task.Delay(AliveCheckTimeOut);
            }
            while (Network.GetState());
        }

        public async void RunSyncTime()
        {
            while (Network.GetState())
            {
                SocketServer.Utils.SystemPackets.SystemTime.Send(this);
                await Task.Delay(TimeSyncTimeOut);
            }
        }

        internal new void AddWaitPacket(byte[] packet_data, int offset, int lenght)
        {
            base.AddWaitPacket(packet_data, offset, lenght);
        }

        public void Send(OutputPacketBuffer packet
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
        {
#if DEBUG
            Network.Send(packet, memberName, sourceFilePath, sourceLineNumber);
#else
            Network.Send(packet);
#endif
        }

        public virtual void ChangeOwner(IServerNetworkClient client)
        {
        }
    }
}
