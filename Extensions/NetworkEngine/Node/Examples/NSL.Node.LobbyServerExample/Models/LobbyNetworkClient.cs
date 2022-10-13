using NSL.WebSockets.Server.AspNetPoint;

namespace NSL.Node.LobbyServerExample.Models
{
    public class LobbyNetworkClient : AspNetWSNetworkServerClient
    {
        public Guid UID { get; set; }

        public Guid CurrentRoomId { get; set; }

        public LobbyRoomInfoModel CurrentRoom { get; set; }
    }
}
