using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer.Utils.SystemPackets;
using System.Threading.Tasks;

namespace SocketServer.Utils
{
    public class IServerNetworkClient : INetworkClient
    {
        public ServerOptions ServerOptions { get; set; }

        public async void RunAliveChecker()
        {
            await Task.Delay(AliveCheckTimeOut);
            while (Network.GetState())
            {
                AliveConnection<IServerNetworkClient>.Send(this);
                await Task.Delay(AliveCheckTimeOut);
            }
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

        public void Send(OutputPacketBuffer packet)
        {
            Network.Send(packet);
        }

        public virtual void ChangeOwner(IServerNetworkClient client)
        {
        }
    }
}
