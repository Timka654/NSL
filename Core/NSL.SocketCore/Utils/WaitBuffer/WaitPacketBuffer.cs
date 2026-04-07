using System.Collections.Generic;
using System.Threading;

namespace NSL.SocketCore.Utils.WaitBuffer
{
    public class WaitPacketBuffer
    {
        public const string DefaultObjectBagKey = NSLObjectBagKeys.WaitPacketBuffer;

        private readonly Queue<PacketWaitInfo> collection = new Queue<PacketWaitInfo>();
        private readonly AutoResetEvent locker;

        public WaitPacketBuffer(bool useLocker = true)
        {
            if (useLocker)
                locker = new AutoResetEvent(true);
        }

        public WaitPacketBuffer(WaitPacketBuffer other, bool useLocker = true) : this(useLocker)
        {
            Append(other);
        }

        public void Clear() => collection.Clear();

        public void Append(WaitPacketBuffer other)
        {
            locker?.WaitOne();

            PacketWaitInfo item;
            while ((item = other.collection.Count > 0 ? other.collection.Peek() : null) != null)
            {
                other.collection.Dequeue();
                collection.Enqueue(item);
            }

            locker?.Set();
        }

        public void Append(byte[] buf, int offset, int len)
        {
            locker?.WaitOne();
            collection.Enqueue(new PacketWaitInfo(buf, offset, len));
            locker?.Set();
        }

        public void Process(BaseNetworkConnection client)
        {
            locker?.WaitOne();

            var col = new Queue<PacketWaitInfo>(collection.ToArray());
            collection.Clear();

            locker?.Set();

            while (col.Count > 0)
            {
                var item = col.Dequeue();
                client.Send(item.Buffer, item.Offset, item.Len);
            }
        }
    }
}
