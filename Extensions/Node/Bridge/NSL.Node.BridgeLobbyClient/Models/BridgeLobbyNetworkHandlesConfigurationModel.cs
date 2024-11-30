using System;
using System.Threading.Tasks;

namespace NSL.Node.BridgeLobbyClient.Models
{
    public class BridgeLobbyNetworkHandlesConfigurationModel
    {
        public RoomDataMessageDelegate RoomFinishHandle { set; internal get; } = (room, sessionIdentity) => Task.CompletedTask;

        public RoomDataMessageDelegate RoomMessageHandle { set; internal get; } = (room, sessionIdentity) => Task.CompletedTask;
    }

    public delegate Task RoomDataMessageDelegate(Guid roomId, byte[] data);
}
