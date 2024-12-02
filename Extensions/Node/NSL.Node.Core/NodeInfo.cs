using NSL.SocketCore.Utils;
using System;

namespace NSL.Node.RoomServer.Shared.Client.Core
{
    public partial class NodeInfo
    {
        public INodeClientNetwork Network { get; private set; }

        public string Id { get; }

        public ClientObjectBag ObjectBag => Network.ObjectBag;

        public void InitializeObjectBag()
            => Network.InitializeObjectBag();

        public NodeInfo(INodeClientNetwork network, string id)
        {
            Network = network;
            Id = id;
        }

        public NodeInfo ChangeTo(INodeClientNetwork another)
        {
            Network = another;

            return this;
        }

        public override string ToString()
        {
            return $"(Node: {Id})";
        }
    }
}
