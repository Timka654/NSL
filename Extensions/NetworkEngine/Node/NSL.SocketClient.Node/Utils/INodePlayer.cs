//using ReliableNetcode;

using NSL.SocketServer.Utils;

namespace NSL.SocketClient.Node.Utils
{
    public abstract class INodePlayer
    {
        public virtual string PlayerId { get; internal set; }

        public virtual IServerNetworkClient Network { get; set; }
    }
}
