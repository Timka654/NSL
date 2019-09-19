using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCL.Node.UDPNode
{
    internal class UDPServerTransportSink : IServerChannelSink
    {
        private IServerChannelSink m_Next;

        internal UDPServerTransportSink(IServerChannelSink Next)
        {
            m_Next = Next;
        }

        #region IServerChannelSink Members

        public System.IO.Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
        {
            return null;
        }

        public System.Runtime.Remoting.Channels.ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, System.IO.Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out System.IO.Stream responseStream)
        {
            responseMsg = null;

            responseHeaders = null;

            responseStream = null;

            throw new NotSupportedException();
        }

        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, System.IO.Stream stream)
        {
            throw new NotSupportedException();
        }

        public IServerChannelSink NextChannelSink
        {
            get
            {
                return m_Next;
            }
        }

        #endregion

        #region IChannelSinkBase Members

        public IDictionary Properties
        {
            get
            {
                return null;
            }
        }

        #endregion

        public void RunServer(int Port)
        {
            IPEndPoint LocalEP = new IPEndPoint(IPAddress.Any, Port);

            Socket ReceivingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            ReceivingSocket.Bind(LocalEP);

            for (int i = 0; i < 5; i++)
            {
                ServerRequestState State = new ServerRequestState();

                State.S = ReceivingSocket;

                IPEndPoint RemoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

                SocketAddress RemoteAddress = new SocketAddress(AddressFamily.InterNetwork);

                State.RemoteEP = RemoteIPEndPoint.Create(RemoteAddress);

                State.S.BeginReceiveFrom(State.RequestBuffer, 0, State.RequestBuffer.Length, SocketFlags.None,
                    ref State.RemoteEP, new AsyncCallback(ServerRequestHandler), State);
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private class ServerRequestState
        {
            public byte[] RequestBuffer = new byte[65536];

            public Socket S = null;

            public EndPoint RemoteEP = null;
        }

        private void ServerRequestHandler(IAsyncResult ar)
        {
            ServerRequestState State = (ServerRequestState)ar.AsyncState;

            State.S.EndReceiveFrom(ar, ref State.RemoteEP);

            ITransportHeaders RequestHeaders;

            Stream RequestStream;

            try
            {
                UDPChannelIO.PrepareInboundMessage(State.RequestBuffer, out RequestHeaders, out RequestStream);

                // Setup a sink stack to pass to Process message in the next sink
                ServerChannelSinkStack SinkStack = new ServerChannelSinkStack();

                SinkStack.Push(this, null);

                // Setup the response to hand back to the client
                IMessage ResponseMessage;

                ITransportHeaders ResponseHeaders;

                Stream ResponseStream;

                // Call the upstream sinks process message
                ServerProcessing Processing = this.NextChannelSink.ProcessMessage(
                    SinkStack,
                    null,
                    RequestHeaders,
                    RequestStream,
                    out ResponseMessage,
                    out ResponseHeaders,
                    out ResponseStream);

                // handle response

                switch (Processing)
                {
                    case ServerProcessing.Complete:

                        // Call completed synchronously send the response immediately
                        SinkStack.Pop(this);

                        // Prepare response to send back to client
                        byte[] SendBuffer;

                        UDPChannelIO.PrepareOutboundMessage("", ResponseHeaders, ResponseStream, out SendBuffer);

                        int BytesSent = State.S.SendTo(SendBuffer, State.RemoteEP);
                        break;
                    case ServerProcessing.OneWay:
                        break;
                    case ServerProcessing.Async:
                        SinkStack.StoreAndDispatch(this, null);
                        break;
                }
            }
            catch (Exception)
            {
                // Problem with inbound message - just ignore and continue.
            }

            State.S.BeginReceiveFrom(State.RequestBuffer, 0, State.RequestBuffer.Length, SocketFlags.None,
                ref State.RemoteEP, new AsyncCallback(ServerRequestHandler), State);
        }

    }
}
