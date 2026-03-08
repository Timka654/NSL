using System;

namespace NSL.Node.Core
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
