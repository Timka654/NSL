using NSL.WebSockets.Server.AspNetPoint;

namespace NSL.Node.LobbyServerExample.Shared.Models
{
    public class LobbyNetworkClientModel : AspNetWSNetworkServerClient
    {
        public Guid UID { get; set; }

        public Guid CurrentRoomId { get; set; }

        public LobbyRoomInfoModel CurrentRoom { get; set; }
    }
}
