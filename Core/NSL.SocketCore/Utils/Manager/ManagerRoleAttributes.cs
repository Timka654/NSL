namespace NSL.SocketCore.Utils.Manager
{
    public class ApiServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public ApiServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class AuthServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public AuthServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class ClientServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public ClientServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class GameServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public GameServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class LobbyServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public LobbyServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class LoginServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public LoginServerManagerLoadAttribute(int offset) : base(offset) { }
    }

    public class RoomServerManagerLoadAttribute : ManagerLoadAttribute
    {
        public RoomServerManagerLoadAttribute(int offset) : base(offset) { }
    }
}
