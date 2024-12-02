using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Data;

namespace NSL.Node.LocalRoomServerExample
{
    public class LocalAspRoomServerStartupEntry : RoomServerHandleProcessor
    {
        private const string LocationId = "";

        public override Task<RoomSignSessionResponseModel> ValidateSession(RoomSignSessionRequestModel request)
        {
            var options = new NodeRoomStartupInfo();

            options.SetRoomNodeCount(1);
            options.SetRoomWaitReady(false);

            options.SetDestroyOnEmpty(true);


            options.SetValue("LocationId", LocationId);
            //need only for network lobby sign
            options.SetValue("LobbyEndPointUrl", string.Empty);
            options.SetValue("AuthorizeKey", string.Empty);


            return Task.FromResult(new RoomSignSessionResponseModel() { Result = true, RoomId = request.RoomIdentity, Options = options.GetDictionary() });
        }

        public override Task<RoomSignSessionPlayerResponseModel> ValidateSessionPlayer(RoomSignSessionPlayerRequestModel request)
        {
            return Task.FromResult(new RoomSignSessionPlayerResponseModel() { ExistsSession = true, ExistsPlayer = true });
        }

        public override void FinishRoomHandle(RoomInfo room, byte[] data)
        {
            // ignore on local
        }

        public override void RoomMessageHandle(RoomInfo room, byte[] data)
        {
            // ignore on local
        }

        public override void OnBridgeStateChangeHandle(bool state)
        {
            // ignore on local
        }
    }
}
