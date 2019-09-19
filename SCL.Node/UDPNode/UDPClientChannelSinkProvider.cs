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
    internal class UDPClientChannelSinkProvider : IClientChannelSinkProvider
    {
        #region IClientChannelSinkProvider Members

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            return new UDPClientTransportSink(url);
        }

        public IClientChannelSinkProvider Next
        {
            get
            {
                // In the client, we are at the end of the sink chain in the client so return null.
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}
