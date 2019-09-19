using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SCL.Node.UDPNode
{
    internal class UDPClientTransportSink : IClientChannelSink
    {
        private Socket m_Socket = null;

        private IPEndPoint m_ServerEp = null;

        internal UDPClientTransportSink(string url)
        {
            int Port = 5150; // default port

            string Server = null;

            System.Uri ParsedURI = new Uri(url);

            if (ParsedURI.Port != -1)
            {
                Port = ParsedURI.Port;
            }

            Server = ParsedURI.Host;

            IPHostEntry IPHost = Dns.GetHostEntry(Server);

            m_ServerEp = new IPEndPoint(IPHost.AddressList[0], Port);

            m_Socket = new Socket(

                AddressFamily.InterNetwork,

                SocketType.Dgram,

                ProtocolType.Udp

                );
        }

        ~UDPClientTransportSink()
        {
            if (m_Socket != null)
            {
                m_Socket.Close();

                m_Socket = null;
            }
        }

        #region IClientChannelSink Members

        public void AsyncProcessRequest(IClientChannelSinkStack SinkStack,
            IMessage Msg,
            ITransportHeaders RequestHeaders,
            System.IO.Stream RequestStream)
        {
            IMethodCallMessage MCM = (IMethodCallMessage)Msg;

            string Uri = MCM.Uri;

            MethodBase MB = MCM.MethodBase;

            byte[] RequestBuffer = null;

            UDPChannelIO.PrepareOutboundMessage(Uri,
                RequestHeaders,
                RequestStream,
                out RequestBuffer
                );

            // We will create a new client socket to handle a specific asynchronous request.
            Socket S = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
                );



            // NOTE: UDP is a connectionless protocol that does not ensure the data

            // you send is received by a peer. As a result, to make UDP reliable you

            // have to implement a scheme where a client and server transmits and

            // receives acknowledgements that indicate the peer has received the message.

            // This sample DOES NOT handle making UDP data transmission reliable therefore

            // it is possible that a client may stop communicating with the server because

            // of packet loss in this sample. It is up to the developer to expand the

            // capabilities of this sample to implement reliability over UDP.

            S.SendTo(RequestBuffer, m_ServerEp);

            bool OneWay = RemotingServices.IsOneWay(MB);

            if (OneWay)
            {
                S.Close();
            }
            else
            {
                AsyncProcessRequestState State = new AsyncProcessRequestState();

                State.S = S;

                State.SinkStack = SinkStack;

                IPEndPoint RemoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

                SocketAddress RemoteAddress = new SocketAddress(AddressFamily.InterNetwork);

                EndPoint RemoteEP = RemoteIPEndPoint.Create(RemoteAddress);

                S.BeginReceiveFrom(State.ResponseBuffer, 0, State.ResponseBuffer.Length, SocketFlags.None, ref RemoteEP,
                    new AsyncCallback(AsyncProcessRequestHandler), State);
            }
        }

        private class AsyncProcessRequestState
        {
            public byte[] ResponseBuffer = new byte[65536];

            public IClientChannelSinkStack SinkStack = null;

            public Socket S = null;
        }

        private void AsyncProcessRequestHandler(IAsyncResult ar)
        {
            AsyncProcessRequestState State = (AsyncProcessRequestState)ar.AsyncState;

            EndPoint RemoteEP = null;

            if (State.S.EndReceiveFrom(ar, ref RemoteEP) > 0)
            {
                ITransportHeaders ResponseHeaders = null;

                System.IO.Stream ResponseStream = null;

                UDPChannelIO.PrepareInboundMessage(State.ResponseBuffer, out ResponseHeaders, out ResponseStream);

                State.SinkStack.AsyncProcessResponse(ResponseHeaders, ResponseStream);
            }

            State.S.Close();
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state,
            ITransportHeaders headers,
            System.IO.Stream stream)
        {
            // We are last in the chain - no need to implement
            throw new NotSupportedException();
        }

        public System.IO.Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
        {
            // We don't do any serialization here.
            return null;
        }

        public void ProcessMessage(IMessage Msg,
            ITransportHeaders RequestHeaders,
            System.IO.Stream RequestStream,
            out ITransportHeaders ResponseHeaders,
            out System.IO.Stream ResponseStream)
        {
            IMethodCallMessage MCM = (IMethodCallMessage)Msg;

            string Uri = MCM.Uri;

            byte[] RequestBuffer = null;

            UDPChannelIO.PrepareOutboundMessage(Uri,

                RequestHeaders,

                RequestStream,

                out RequestBuffer

                );

            IPEndPoint RemoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            SocketAddress RemoteAddress = new SocketAddress(AddressFamily.InterNetwork);

            EndPoint RemoteEP = RemoteIPEndPoint.Create(RemoteAddress);

            byte[] ResponseBuffer = new byte[65536];

            // NOTE: UDP is a connectionless protocol that does not ensure the data

            // you send is received by a peer. As a result, to make UDP reliable you

            // have to implement a scheme where a client and server transmits and

            // receives acknowledgements that indicate the peer has received the message.

            // This sample DOES NOT handle making UDP data transmission reliable therefore

            // it is possible that a client may stop communicating with the server because

            // of packet loss in this sample. It is up to the developer to expand the

            // capabilities of this sample to implement reliability over UDP.

            m_Socket.SendTo(RequestBuffer, m_ServerEp);

            int BytesReceived = m_Socket.ReceiveFrom(ResponseBuffer, ref RemoteEP);

            UDPChannelIO.PrepareInboundMessage(ResponseBuffer, out ResponseHeaders, out ResponseStream);
        }

        public IClientChannelSink NextChannelSink
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IChannelSinkBase Members

        public System.Collections.IDictionary Properties
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}
