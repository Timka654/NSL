using NSL.SocketClient.Node.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NSL.SocketClient.Node
{
    public class INetworkNode
    {
        public string PlayerId { get; internal set; }

        public delegate void CommandHandle(INodePlayer player, NodeInputPacketBuffer buffer);

        protected readonly Dictionary<ushort, CommandHandle> _commands = new Dictionary<ushort, CommandHandle>();

        public virtual void AddPacketHandle(ushort id, CommandHandle handle)
        {
            if (_commands.ContainsKey(id))
            {
                _commands.Remove(id);
            }

            _commands.Add(id, handle);
        }
    }

    public class INetworkNode<T> : INetworkNode
        where T : INodePlayer
    {
        public int Port { get; protected set; }

        protected readonly ConcurrentDictionary<string, T> _players = new ConcurrentDictionary<string, T>();

        public virtual bool Initialize(string ip, ref int port, string myPlayerId)
        {
            PlayerId = myPlayerId;

            return true;
        }
    }
}
