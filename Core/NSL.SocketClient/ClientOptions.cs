using NSL.SocketClient.Utils;
using NSL.SocketClient.Utils.SystemPackets;
using NSL.SocketCore;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.SystemPackets;
using System;
using System.Net.Sockets;

namespace NSL.SocketClient
{
    public class ClientOptions<TClient> : TypedCoreOptions<TClient>
        where TClient : BaseNetworkConnection, new()
    {
        #region EventDelegates

        public delegate void ExtensionHandleDelegate(Exception ex, TClient client);
        public delegate void ReconnectDelegate(int currentTry, bool result);
        public delegate void ClientConnectedDelegate(TClient client);
        public delegate void ClientDisconnectedDelegate(TClient client);

        #endregion

        public ClientOptions()
        {
            ConnectionFactory = () => new TClient();

            AddPacket(ClientSystemTimePacket.PacketId, new ClientSystemTimePacket<TClient>(this));
            AddPacket(AliveConnectionPacket.PacketId, new ClientAliveConnectionPacket<TClient>(this));
        }

        public override void RunException(Exception ex) => base.CallExceptionEvent(ex, ClientData);

        public override void RunClientConnect() => base.CallClientConnectEvent(ClientData);

        public override void RunClientDisconnect()
        {
            foreach (var packet in Packets.Values)
                (packet as ILockedPacket)?.UnlockPacket();

            OnRunClientDisconnect();
        }

        protected virtual void OnRunClientDisconnect() => CallClientDisconnectEvent(ClientData);

        public IClient NetworkClient => ClientData?.Network;

        public override void InitializeClient(BaseNetworkConnection newClientData)
        {
            if (newClientData == null)
            {
                base.ClientData = null;
                return;
            }

            var oldCD = ClientData;

            base.ClientData = (TClient)newClientData;

            if (oldCD != null)
            {
                ClientData.Network = oldCD.Network;
                oldCD.Network = null;
                ClientData.ChangeOwner(oldCD);
            }
        }

        public void InitializeClient(TClient newClientData) => InitializeClient((BaseNetworkConnection)newClientData);

        public bool AddPacket(ushort packetId, IClientPacket<TClient> packet)
            => AddPacketHandle(packetId, (IPacket<TClient>)packet);

        public void InitializeClientObjectBagOnConnect()
        {
            base.OnClientConnectEvent += c => c.InitializeObjectBag();
        }
    }

    public static class NetworkConfigurationExtension
    {
        public static ClientOptions<T> LoadConfigurationClientOptions<T>(this INSLConfiguration configuration, string networkNodePath)
            where T : BaseNetworkConnection, new()
        {
            var r = configuration.LoadConfigurationCoreOptions<ClientOptions<T>>(networkNodePath);
            r.WithRemoteEndPoint(
                configuration.GetValue($"{networkNodePath}.io.ip"),
                configuration.GetValue<int>($"{networkNodePath}.io.port"));
            return r;
        }
    }
}
