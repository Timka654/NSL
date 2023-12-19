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
using UnityEngine.AI;

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

            lastReceivedSegment = null;
            openedCTS.Cancel();
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

        byte[] lastReceivedSegment = null;

        private int TryWriteSegment(ArraySegment<byte> buf, int offset)
        {
            var copyLen = buf.Count - offset;

            if (copyLen > lastReceivedSegment.Length)
                copyLen = lastReceivedSegment.Length;

            Buffer.BlockCopy(lastReceivedSegment, 0, buf.Array, buf.Offset + offset, copyLen);

            if (copyLen < lastReceivedSegment.Length)
            {
                var latestRecvSegment = new byte[lastReceivedSegment.Length - copyLen];

                Buffer.BlockCopy(lastReceivedSegment, copyLen, latestRecvSegment, 0, latestRecvSegment.Length);

                lastReceivedSegment = latestRecvSegment;
            }
            else
                lastReceivedSegment = null;

            return copyLen;
        }

        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (state != WebSocketState.Open)
                throw new Exception("Socket first must be opened for receive");

            var offset = 0;

            if (lastReceivedSegment != null)
            {
                offset += TryWriteSegment(buffer, offset);
            }

            while (offset < buffer.Count && !cancellationToken.IsCancellationRequested && !openedCTS.IsCancellationRequested)
            {
                if (!receiveQueue.TryDequeue(out var newSegment))
                    await Task.Yield();
                else
                {
                    lastReceivedSegment = newSegment;

                    offset += TryWriteSegment(buffer, offset);
                }
            }

            if (cancellationToken.IsCancellationRequested || openedCTS.IsCancellationRequested)
                return new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true, CloseStatus, CloseStatusDescription);

            return new WebSocketReceiveResult(buffer.Count, WebSocketMessageType.Binary, true, CloseStatus, CloseStatusDescription);
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

            SimpleWebJSLib.Connect(endPoint.ToString(), OpenCallback, CloseCallBack, MessageCallback, ErrorCallback, index);

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

        private ConcurrentQueue<byte[]> receiveQueue = new ConcurrentQueue<byte[]>();

        void onMessage(IntPtr bufferPtr, int count)
        {
            try
            {
                byte[] data = new byte[count];

                Marshal.Copy(bufferPtr, data, 0, count);

                receiveQueue.Enqueue(data);
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
