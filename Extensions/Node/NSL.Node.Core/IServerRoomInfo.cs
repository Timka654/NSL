using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public interface IServerRoomInfo : IRoomInfo
    {
        void SendLobbyFinishRoom(byte[] data = null);

        void SendLobbyRoomMessage(byte[] data);

        TValue GetOption<TValue>(string key)
            where TValue : IConvertible;

        void Dispose(byte[] data);

        bool ValidateSession(NodeInfo node);
    }
}
