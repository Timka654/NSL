namespace NSL.Node.Core
{
    public interface INodeOwneredObject
    {
        IRoomInfo NodeRoom { get; }
        INodeClientNetwork NodePlayer { get; }

        void SetOwner(IRoomInfo nodeRoom, INodeClientNetwork nodePlayer);
    }
}
