using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient.Utils
{
    public class IPacketReceive<TClient, RData, SPType> : IPacketReceive<TClient, RData> where TClient : BaseSocketNetworkClient where SPType : struct, IConvertible
    {
        public IPacketReceive(ClientOptions<TClient> options) : base(options)
        {
        }

        protected void Send(SPType packetId)
        {
            Send(Convert.ToUInt16(packetId));
        }

        protected RData SendWait(SPType packetId)
        {
            return SendWait(Convert.ToUInt16(packetId));
        }

        protected async Task<RData> SendWaitAsync(SPType packetId)
        {
            return await SendWaitAsync(Convert.ToUInt16(packetId));
        }
    }

    public class IPacketReceive<TClient, RData> : IClientPacket<TClient>, ILockedPacket where TClient : BaseSocketNetworkClient
    {
        public IPacketReceive(ClientOptions<TClient> options) : base(options)
        {
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
        }

        private void Options_OnClientDisconnectEvent(TClient client)
        {
            UnlockPacket();
        }

        protected AutoResetEvent _dataLocker = new AutoResetEvent(true);

        private RData _data;

        protected RData Data
        {
            get
            {
                _dataLocker.WaitOne();
                _dataLocker.Set();
                return _data;
            }
            set
            {
                _data = value;
                _dataLocker.Set();
            }
        }

        protected void Send(ushort packetId)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            Send(packet);
        }

        protected RData SendWait(OutputPacketBuffer packet)
        {
            _dataLocker.WaitOne();

            base.Send(packet);

            return Data;
        }

        protected RData SendWait(ushort packetId)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            return SendWait(packet);
        }

        protected async Task<RData> SendWaitAsync(OutputPacketBuffer packet)
        {
            return await Task.Run(() => SendWait(packet));
        }

        protected async Task<RData> SendWaitAsync(ushort packetId)
        {
            return await Task.Run(() => SendWait(packetId));
        }

        public void UnlockPacket()
        {
            Data = default;
        }
    }
}
