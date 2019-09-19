using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCL.Node.UDPNode
{
    public class UDPChannel : IChannelReceiver, IChannelSender
    {
        private IClientChannelSinkProvider m_ClientSinkProvidersChain = null;

        private UDPServerTransportSink m_ServerTransportSink;

        private int m_ChannelPriority = 1;

        private string m_ChannelName = "udp";

        private int m_ChannelPort = 5150; // default port

        private Thread m_ServerThread = null;

        private ChannelDataStore m_ChannelDataStore;

        // #region <label>...#endregion used for the collapsible code portion

        // for more readable and organized codes

        #region Constructors

        // This constructor is used by a client application to

        // programmatically configure the client side of the remoting channel.

        public UDPChannel()
        {
            SetupClientSinkProviders(null);
        }

        // This constructor is used by a server application to

        // programmatically configure the server side of the remoting channel.

        public UDPChannel(int Port)
            : this()
        {
            m_ChannelPort = Port;

            SetupServerSinkProviders(null);
        }

        // This constructor is used by the .Net remoting infrastructure

        // to configure the channel via a configuration file.
        public UDPChannel(
            IDictionary Properties,
            IClientChannelSinkProvider ClientProviderChain,
            IServerChannelSinkProvider ServerProviderChain
            )
        {
            if (Properties != null)
            {
                foreach (DictionaryEntry Entry in Properties)
                {
                    switch ((string)Entry.Key)
                    {
                        case "name":
                            m_ChannelName = (string)Entry.Value;
                            break;
                        case "priority":
                            m_ChannelPriority = Convert.ToInt32(Entry.Value);
                            break;
                        case "port":
                            m_ChannelPort = Convert.ToInt32(Entry.Value);
                            break;
                    }
                }
            }

            SetupClientSinkProviders(ClientProviderChain);
            SetupServerSinkProviders(ServerProviderChain);
        }

        #endregion

        internal void SetupClientSinkProviders(IClientChannelSinkProvider ClientProviderChain)
        {
            if (ClientProviderChain == null)
            {
                // Install at least default formatter for serialization
                m_ClientSinkProvidersChain = new BinaryClientFormatterSinkProvider();
            }
            else
            {
                // Get the provider chain from the outside
                m_ClientSinkProvidersChain = ClientProviderChain;
            }

            // Move to the end of the sink provider chain
            IClientChannelSinkProvider TempSinkProvider = m_ClientSinkProvidersChain;

            while (TempSinkProvider.Next != null)
                TempSinkProvider = TempSinkProvider.Next;

            // Append our new UDP channel sink to the end

            TempSinkProvider.Next = new UDPClientChannelSinkProvider();
        }

        internal void SetupServerSinkProviders(IServerChannelSinkProvider InputSinkProvider)
        {
            string MachineName = Dns.GetHostName();

            m_ChannelDataStore = new ChannelDataStore(null);

            m_ChannelDataStore.ChannelUris = new string[1];

            m_ChannelDataStore.ChannelUris[0] = m_ChannelName + "://" + MachineName +
                ":" + m_ChannelPort.ToString();

            IServerChannelSinkProvider ServerSinkProvidersChain;

            // Create a default sink provider if one was not passed in

            if (InputSinkProvider == null)
            {
                ServerSinkProvidersChain = new BinaryServerFormatterSinkProvider();
            }
            else
            {
                ServerSinkProvidersChain = InputSinkProvider;
            }

            // Collect the rest of the channel data:
            IServerChannelSinkProvider provider = ServerSinkProvidersChain;

            while (provider != null)
            {
                provider.GetChannelData(m_ChannelDataStore);

                provider = provider.Next;
            }

            // Create a chain of sink providers
            IServerChannelSink next = ChannelServices.CreateServerChannelSinkChain(ServerSinkProvidersChain, this);

            m_ServerTransportSink = new UDPServerTransportSink(next);

            StartListening(null);

        }

        #region IChannel Members

        public string ChannelName
        {
            get
            {
                return m_ChannelName;
            }
        }

        public int ChannelPriority
        {
            get
            {
                return m_ChannelPriority;
            }
        }

        public string Parse(string Url, out string ObjectUri)
        {
            return UDPChannelIO.Parse(Url, out ObjectUri);
        }

        #endregion

        #region IChannelSender Members

        public IMessageSink CreateMessageSink(string Url, object RemoteChannelData, out string ObjectUri)
        {
            // Set the out parameters
            ObjectUri = null;

            string ChannelUri = null;

            if (Url != null)
            {
                ChannelUri = Parse(Url, out ObjectUri);
            }
            else
            {
                if (RemoteChannelData != null)
                {
                    IChannelDataStore DataStore = RemoteChannelData as IChannelDataStore;

                    if (DataStore != null)
                    {
                        ChannelUri = Parse(DataStore.ChannelUris[0], out ObjectUri);

                        if (ChannelUri != null)
                            Url = DataStore.ChannelUris[0];
                    }
                }
            }

            if (ChannelUri != null)
            {
                if (Url == null)
                    Url = ChannelUri;

                // Return the first sink of the newly formed sink chain
                return (IMessageSink)m_ClientSinkProvidersChain.CreateSink(this, Url, RemoteChannelData);
            }

            return null;
        }

        #endregion

        #region IChannelReceiver Members

        public void StartListening(object data)
        {
            m_ServerThread = new Thread(new ThreadStart(this.ServerThreadRoutine));

            m_ServerThread.IsBackground = true;

            m_ServerThread.Start();
        }

        public object ChannelData
        {
            get
            {
                return m_ChannelDataStore;
            }
        }

        public void StopListening(object data)
        {
            if (m_ServerThread != null)
            {
                m_ServerThread.Abort();

                m_ServerThread = null;
            }
        }

        public string[] GetUrlsForUri(string ObjectUri)
        {
            string[] UrlArray = new string[1];

            if (!ObjectUri.StartsWith("/"))
                ObjectUri = "/" + ObjectUri;

            string MachineName = Dns.GetHostName();

            UrlArray[0] = m_ChannelName +
                "://" + MachineName + ":" +
                m_ChannelPort + ObjectUri;

            return UrlArray;
        }

        #endregion

        private void ServerThreadRoutine()
        {
            m_ServerTransportSink.RunServer(m_ChannelPort);
        }
    }
}
