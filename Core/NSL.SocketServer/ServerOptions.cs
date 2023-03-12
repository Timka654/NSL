using NSL.SocketCore.Utils.SystemPackets;
using NSL.SocketCore;
using NSL.SocketServer.Utils;
using NSL.SocketServer.Utils.SystemPackets;
using System.Net;

namespace NSL.SocketServer
{
    public class ServerOptions<TClient> : CoreOptions<TClient>
        where TClient : IServerNetworkClient
    {
        #region ServerSettings
        //Данные для настройки сервера

        /// <summary>
        /// Длина очереди для приема подключения
        /// </summary>
        public virtual int Backlog { get; set; }

        #endregion

        public event OnRecoverySessionReceiveDelegate<TClient> OnRecoverySessionReceiveEvent
        {
            add { GetPacket<RecoverySessionPacket<TClient>>(RecoverySessionPacket<TClient>.PacketId).OnRecoverySessionReceiveEvent += value; }
            remove { GetPacket<RecoverySessionPacket<TClient>>(RecoverySessionPacket<TClient>.PacketId).OnRecoverySessionReceiveEvent -= value; }
        }

        public ServerOptions()
        {
            LoadOptions();
        }

        protected virtual void LoadOptions()
        { 
            AddPacket(AliveConnectionPacket.PacketId, new ServerAliveConnectionPacket<TClient>());
            AddPacket(RecoverySessionPacket<TClient>.PacketId, new RecoverySessionPacket<TClient>());
            AddPacket(VersionPacket<TClient>.PacketId, new VersionPacket<TClient>());
            AddPacket(SystemTime<TClient>.PacketId, new SystemTime<TClient>());
        }

        /// <summary>
        /// Ип адресс - используется для инициализации слушателя на определенном адаптере (0.0.0.0 - для всех)
        /// </summary>
        public virtual string IpAddress { get; set; } = "0.0.0.0";

        /// <summary>
        /// Порт - используется для инициализации слушателя на определенном порту 1 - 65,535
        /// </summary>
        public virtual int Port { get; set; }

        public IPAddress GetIPAddress() => IPAddress.Parse(IpAddress);

        public IPEndPoint GetIPEndPoint() => new IPEndPoint(GetIPAddress(), Port);
    }
}
