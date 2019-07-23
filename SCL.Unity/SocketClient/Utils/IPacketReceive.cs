using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SCL.SocketClient.Utils.Buffer;

namespace SCL.SocketClient.Utils
{
    public class IPacketReceive<TClient, RData, SPType> : IPacketReceive<TClient, RData> where TClient : BaseSocketNetworkClient where SPType: struct, IConvertible 
    {
        public IPacketReceive(ClientOptions<TClient> options) : base(options)
        {
        }

        protected async Task<RData> SendSerializeAsync<T>(SPType packetId, T obj, string scheme)
        {
            return await base.SendSerializeAsync(Convert.ToUInt16(packetId), obj, scheme);
        }

        protected async Task<RData> SendEmptyAsync(SPType packetId)
        {
            return await Task.Run(() => SendEmpty(Convert.ToUInt16(packetId)));
        }

    }

    public class IPacketReceive<TClient, RData> : IPacket<TClient>,ILockedPacket where TClient : BaseSocketNetworkClient
    {
        public IPacketReceive(ClientOptions<TClient> options) : base(options)
        {
            options.OnClientDisconnectEvent += Options_OnClientDisconnectEvent;
        }

        private void Options_OnClientDisconnectEvent(TClient client)
        {
            Data = default(RData);
        }

        protected AutoResetEvent _dataLocker = new AutoResetEvent(true);

        private RData _data;

        protected RData Data
        {
            get { 
                _dataLocker.WaitOne();
                _dataLocker.Set();
                return _data;
            }
            set { 
                _data = value;
                _dataLocker.Set();
            }
        }

        protected RData Send(OutputPacketBuffer packet)
        {
            _dataLocker.WaitOne();

            base.Send(packet);

            return Data;
        }


        protected RData SendEmpty(ushort packetId)
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            return Send(packet);
        }

        protected RData SendSerialize<T>(ushort packetId, T obj, string scheme)
        {
            _dataLocker.WaitOne();

            base.SendSerialize(packetId, obj, scheme);

            return Data;
        }

        protected async Task<RData> SendAsync(OutputPacketBuffer packet)
        {
            return await Task.Run(() => Send(packet));
        }

        protected async Task<RData> SendEmptyAsync(ushort packetId)
        {
            return await Task.Run(() => SendEmpty(packetId));
        }

        protected async Task<RData> SendSerializeAsync<T>(ushort packetId, T obj, string scheme)
        {
            return await Task.Run(() => SendSerialize(packetId,obj, scheme));
        }

        protected void SendNoLock(OutputPacketBuffer packet)
        {
            base.Send(packet);
        }

        public void UnlockPacket()
        {
            Data = default(RData);
        }
    }
}
