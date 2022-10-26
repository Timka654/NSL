namespace NSL.Node.LobbyServerExample.Shared.Models.Request
{
    public class CreateRoomRequestModel
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public int MaxPlayerCount { get; set; }
    }
}
