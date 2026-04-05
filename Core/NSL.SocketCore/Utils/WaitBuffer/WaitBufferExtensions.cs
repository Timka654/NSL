namespace NSL.SocketCore.Utils.WaitBuffer
{
    public static class WaitBufferExtensions
    {
        public static void AddWaitPacketBuffer<TClient>(this TClient client, bool useLocker = true, string objectBagKey = WaitPacketBuffer.DefaultObjectBagKey)
            where TClient : INetworkClient
        {
            client.ObjectBag.Set(objectBagKey, new WaitPacketBuffer(useLocker));
        }

        public static WaitPacketBuffer GetWaitPacketBuffer<TClient>(this TClient client, string objectBagKey = WaitPacketBuffer.DefaultObjectBagKey)
            where TClient : INetworkClient
            => client.ObjectBag.Get<WaitPacketBuffer>(objectBagKey, true);
    }
}
