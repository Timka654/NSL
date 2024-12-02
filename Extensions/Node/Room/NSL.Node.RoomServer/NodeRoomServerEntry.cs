using NSL.Logger.Interface;
using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Shared.Client.Core;
using System;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer
{
    public class NodeRoomServerEntry
    {
        public delegate Task<RoomSignSessionResponseModel> ValidateSessionDelegate(RoomSignSessionRequestModel query);
        public delegate Task<RoomSignSessionPlayerResponseModel> ValidateSessionPlayerDelegate(RoomSignSessionPlayerRequestModel query);

        public delegate void RoomFinishHandleDelegate(RoomInfo roomInfo, byte[] data);
        public delegate void RoomMessageHandleDelegate(RoomInfo roomInfo, byte[] data);

        public delegate void StateChangeDelegate(bool state);

        public delegate IRoomSession CreateSessionDelegate(IServerRoomInfo roomInfo);

        internal BridgeRoomBaseNetwork BridgeNetworkClient { get; set; }

        internal ClientServerBaseEntry ClientServerListener { get; set; }

        internal ILogger Logger { get; set; }

        internal ValidateSessionDelegate ValidateSession { get; set; } = (query) => Task.FromResult<RoomSignSessionResponseModel>(null);

        internal ValidateSessionPlayerDelegate ValidateSessionPlayer { get; set; } = (query) => Task.FromResult<RoomSignSessionPlayerResponseModel>(null);

        internal RoomFinishHandleDelegate RoomFinishHandle { get; set; } = (room, data) => { };

        internal RoomMessageHandleDelegate RoomMessageHandle { get; set; } = (room, data) => { };

        internal StateChangeDelegate BridgeConnectionStateChangedHandle { get; set; } = (state) => { };

        internal CreateSessionDelegate CreateRoomSession { get; set; } = (serverInfo) => default;

        internal TimeSpan? ReconnectSessionLifeTime { get; set; } = null;

        internal void Run()
        {
            if (BridgeNetworkClient == null)
            {
                ClientServerListener?.Run();
                return;
            }

            BridgeNetworkClient.OnStateChanged += state =>
            {
                if (state)
                    ClientServerListener?.Run();

                BridgeConnectionStateChangedHandle(state);
            };

            BridgeNetworkClient.Initialize();
        }
    }
}
