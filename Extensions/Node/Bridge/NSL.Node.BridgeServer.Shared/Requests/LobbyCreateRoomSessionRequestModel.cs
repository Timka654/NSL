using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;

namespace NSL.Node.BridgeServer.Shared.Requests
{
    [NSLBIOType]
    public partial class LobbyCreateRoomSessionRequestModel
    {
        public Guid RoomId { get; set; }

        public string Location { get; set; }

        public Guid? SpecialServer { get; set; }

        public List<string>? InitialPlayers { get; set; }

        public int NeedPointCount { get; set; } = 1;

        public int? InstanceWeight { get; set; }

        public Dictionary<string, string> StartupOptions { get; set; }

        public int? DelaySecondsForInactiveDestroy { get; set; }
    }
}
