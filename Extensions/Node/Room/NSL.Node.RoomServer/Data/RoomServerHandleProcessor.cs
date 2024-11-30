using NSL.Node.BridgeServer.Shared.Requests;
using NSL.Node.BridgeServer.Shared.Response;
using NSL.Node.RoomServer.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Data
{
    public abstract class RoomServerHandleProcessor
    {
        public abstract Task<RoomSignSessionResponseModel> ValidateSession(RoomSignSessionRequestModel request);

        public abstract Task<RoomSignSessionPlayerResponseModel> ValidateSessionPlayer(RoomSignSessionPlayerRequestModel request);

        public abstract void FinishRoomHandle(RoomInfo room, byte[] data);

        public abstract void RoomMessageHandle(RoomInfo room, byte[] data); 
        
        public abstract void OnBridgeStateChangeHandle(bool state);
    }
}
