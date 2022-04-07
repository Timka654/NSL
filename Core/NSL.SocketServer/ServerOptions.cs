using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.SystemPackets.Enums;
using SocketServer.Utils;
using SocketServer.Utils.SystemPackets;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SocketServer
{
    public class ServerOptions<TClient> : CoreOptions<TClient>
        where TClient : IServerNetworkClient
    {
        #region ServerSettings
        //Данные для настройки сервера

        /// <summary>
        /// Длина очереди для приема подключения
        /// </summary>
        public int Backlog { get; set; }

        #endregion

        public event OnRecoverySessionReceiveDelegate<TClient> OnRecoverySessionReceiveEvent
        {
            add { GetPacket<RecoverySession<TClient>>((ushort)ServerPacketEnum.RecoverySession).OnRecoverySessionReceiveEvent += value; }
            remove { GetPacket<RecoverySession<TClient>>((ushort)ServerPacketEnum.RecoverySession).OnRecoverySessionReceiveEvent -= value; }
        }

        public ServerOptions()
        {
            AddPacket((ushort)ServerPacketEnum.AliveConnection, new ServerAliveConnection<TClient>());
            AddPacket((ushort)ServerPacketEnum.RecoverySession, new RecoverySession<TClient>());
            AddPacket((ushort)ServerPacketEnum.Version, new Version<TClient>());
            AddPacket((ushort)ServerPacketEnum.ServerTime, new SystemTime<TClient>());
        }



        /// <summary>
        /// Ип адресс - используется для инициализации слушателя на определенном адаптере (0.0.0.0 - для всех)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Порт - используется для инициализации слушателя на определенном порту 1 - 65,535
        /// </summary>
        public int Port { get; set; }

        public IPAddress GetIPAddress() => IPAddress.Parse(IpAddress);

        public IPEndPoint GetIPEndPoint() => new IPEndPoint(GetIPAddress(), Port);
    }
}
