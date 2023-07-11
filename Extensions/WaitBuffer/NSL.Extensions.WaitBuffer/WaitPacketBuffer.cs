using NSL.SocketCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.WaitBuffer
{
    public class WaitPacketBuffer
    {
        public const string DefaultObjectBagKey = "NSL__DEFAULT__WPB";

        Queue<PacketWaitInfo> collection = new Queue<PacketWaitInfo>();

        private AutoResetEvent locker;

        public WaitPacketBuffer(bool useLocker = true) { if(useLocker) locker = new AutoResetEvent(false); }

        public WaitPacketBuffer(WaitPacketBuffer other, bool useLocker = true) : this(useLocker)
        {
            Append(other);
        }

        public void Clear() => collection.Clear();

        public void Append(WaitPacketBuffer other)
        {
            locker?.WaitOne();

            PacketWaitInfo item;
            
            while ((item = other.collection.Peek()) != null)
            {
                other.collection.Dequeue(); collection.Enqueue(item); }

            locker?.Set();
        }

        public void Append(byte[] buf, int offset, int len)
        {
            locker?.WaitOne();

            collection.Enqueue(new PacketWaitInfo(buf, offset, len));

            locker?.Set();
        }

        public void Process(INetworkClient client)
        {
            locker?.WaitOne();

            var col = new Queue<PacketWaitInfo>(collection.ToArray());

            collection.Clear();

            locker?.Set();

            PacketWaitInfo item;

            while ((item = col.Peek()) != null)
            {
                col.Dequeue();
                client.Send(item.Buffer, item.Offset, item.Len);
            }
        }
    }
}
