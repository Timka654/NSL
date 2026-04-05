using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.SocketCore.Utils.Buffer
{
    /// <summary>
    /// Centralized request registry shared by one or more <see cref="RequestProcessor"/> instances.
    /// Reads the request id from an incoming response packet and dispatches it to the
    /// registered handler, allowing multiple processors to operate over a single connection
    /// while still routing responses to their correct owners.
    /// </summary>
    public class RequestProcessorHub : IRequestHub, IDisposable
    {
        private readonly Dictionary<Guid, Action<InputPacketBuffer>> requests
            = new Dictionary<Guid, Action<InputPacketBuffer>>();

        /// <inheritdoc/>
        public Guid CreateRequest(Action<InputPacketBuffer> handler)
        {
            Guid id;
            lock (requests)
            {
                do { id = Guid.NewGuid(); }
                while (requests.ContainsKey(id));

                requests.Add(id, handler);
            }
            return id;
        }

        /// <inheritdoc/>
        public bool TryRemoveRequest(Guid id)
        {
            lock (requests)
            {
                return requests.Remove(id);
            }
        }

        /// <inheritdoc/>
        public bool CancelRequest(Guid id)
        {
            Action<InputPacketBuffer> handler;
            lock (requests)
            {
                if (!requests.TryGetValue(id, out handler))
                    return false;

                requests.Remove(id);
            }

            handler(null);
            return true;
        }

        /// <summary>
        /// Read the request id from <paramref name="data"/> and dispatch the response
        /// to the handler that was registered via <see cref="CreateRequest"/>.
        /// </summary>
        public void ProcessResponse(InputPacketBuffer data)
        {
            var id = data.ReadGuid();

            Action<InputPacketBuffer> handler;
            lock (requests)
            {
                if (!requests.TryGetValue(id, out handler))
                    return;

                requests.Remove(id);
            }

            data.ManualDisposing = true;
            handler(data);
        }

        /// <summary>
        /// Cancel all pending requests, invoking each handler with <see langword="null"/>
        /// to unblock any waiters.
        /// </summary>
        public void Dispose()
        {
            Action<InputPacketBuffer>[] handlers;
            lock (requests)
            {
                handlers = requests.Values.ToArray();
                requests.Clear();
            }

            foreach (var handler in handlers)
                handler(null);
        }
    }
}
