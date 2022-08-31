using AOT;
using NSL.Utils.Unity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine;

namespace NSL.WebSockets.UnityClient
{
    internal class WGLWebSocket : WebSocket
    {
        static readonly Dictionary<int, WGLWebSocket> instances = new Dictionary<int, WGLWebSocket>();

        int index;

        WebSocketCloseStatus? closeStatus;

        string closeStatusDescription;

        private WebSocketState state;

        public override WebSocketCloseStatus? CloseStatus => closeStatus;

        public override string CloseStatusDescription => closeStatusDescription;

        public override WebSocketState State => state;

        public override string SubProtocol => "";

        public WGLWebSocket()
        {
            while (instances.ContainsKey(++index)) ;

            instances.Add(index, this);
        }

        public override void Abort()
        {
            CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);

            lockedSegment = null;
        }

        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            state = WebSocketState.Closed;
            // disconnect should cause closeCallback and OnDisconnect to be called
            SimpleWebJSLib.Disconnect(index);

            instances.Remove(index);

            return Task.CompletedTask;
        }

        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            Abort();
            instances.Remove(index);
        }

        ArraySegment<byte>? lockedSegment = null;

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (state != WebSocketState.Open)
                throw new Exception("Socket first must be opened for receive");

            List<ArraySegment<byte>> array = new List<ArraySegment<byte>>();

            if (lockedSegment.HasValue)
                array.Add(lockedSegment.Value);


            var rcount = buffer.Count;

            int totalCount = 0;

            while ((totalCount = array.Sum(x => x.Count)) < rcount && !cancellationToken.IsCancellationRequested && !openedCTS.IsCancellationRequested)
            {
                if (!receiveQueue.TryDequeue(out var newSegment))
                    await Task.Yield();
                else
                    array.Add(newSegment);
            }

            if (cancellationToken.IsCancellationRequested || openedCTS.IsCancellationRequested)
                return new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true, CloseStatus, CloseStatusDescription);

            var cloneOffset = buffer.Offset;

            foreach (var item in array)
            {
                var needCount = rcount - cloneOffset < item.Count ? buffer.Count - cloneOffset : item.Count;

                Buffer.BlockCopy(item.Array, item.Offset, buffer.Array, cloneOffset, needCount);

                cloneOffset += needCount;
            }


            if (totalCount > rcount)
            {
                var latest = array.Last();
                var offset = rcount - array.Take(array.Count - 1).Sum(x => x.Count);

                if (latest.Count == offset)
                    lockedSegment = latest;
                else
                    lockedSegment = new ArraySegment<byte>(latest.Array, offset, latest.Count - offset);
            }
            else
                lockedSegment = null;

            return new WebSocketReceiveResult(rcount, WebSocketMessageType.Binary, true, CloseStatus, CloseStatusDescription);
        }

        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            if (messageType != WebSocketMessageType.Binary)
                throw new ArgumentOutOfRangeException($"{nameof(WGLWebSocket)} support only \"{nameof(WebSocketMessageType.Binary)}\" {nameof(messageType)}");

            if (!SimpleWebJSLib.Send(index, buffer.Array, buffer.Offset, buffer.Count))
            {

            }

            return Task.CompletedTask;
        }

        private CancellationTokenSource openCTS;
        private CancellationTokenSource openedCTS = new CancellationTokenSource();

        public async Task ConnectAsync(Uri endPoint, CancellationToken cts)
        {
            if (state == WebSocketState.Open)
                return;

            openCTS = new CancellationTokenSource();

            index = SimpleWebJSLib.Connect(endPoint.ToString(), OpenCallback, CloseCallBack, MessageCallback, ErrorCallback);

            state = WebSocketState.Connecting;

            while (!openCTS.IsCancellationRequested)
            {
                await Task.Yield();
            }

            if (!SimpleWebJSLib.IsConnected(index))
                state = WebSocketState.Closed;
            else
            {
                state = WebSocketState.Open;
                openedCTS = new CancellationTokenSource();
            }
        }

        private ConcurrentQueue<ArraySegment<byte>> receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();

        void onMessage(IntPtr bufferPtr, int count)
        {
            try
            {
                byte[] data = new byte[count];

                Marshal.Copy(bufferPtr, data, 0, count);

                receiveQueue.Enqueue(new ArraySegment<byte>(data));
            }
            catch (Exception e)
            {
                CloseAsync(WebSocketCloseStatus.ProtocolError, e.ToString(), CancellationToken.None);
            }
        }

        void onErr()
        {
            CloseAsync(WebSocketCloseStatus.ProtocolError, "Javascript Websocket error", CancellationToken.None);
        }

        #region ExternalHandle

        [MonoPInvokeCallback(typeof(Action<int>))]
        static void OpenCallback(int index) => instances[index].openCTS.Cancel();

        [MonoPInvokeCallback(typeof(Action<int>))]
        static void CloseCallBack(int index) => instances[index].CloseAsync(WebSocketCloseStatus.NormalClosure, "Sockets close", CancellationToken.None);

        [MonoPInvokeCallback(typeof(Action<int, IntPtr, int>))]
        static void MessageCallback(int index, IntPtr bufferPtr, int count) => instances[index].onMessage(bufferPtr, count);

        [MonoPInvokeCallback(typeof(Action<int>))]
        static void ErrorCallback(int index) => instances[index].onErr();

        #endregion
    }
}
