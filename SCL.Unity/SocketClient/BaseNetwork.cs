using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.SocketClient
{
    public class BaseNetwork<T> : MonoBehaviour where T: BaseSocketNetworkClient
    {
        protected virtual string ClientType { get; }

        protected const string TimkaIpAddress = "5.53.116.170";

        public bool Enabled = true;

        public bool LocalServer = false;

        public bool TimkaServer = false;

        public string IpAddress = "5.53.116.170";

        public int Port = 4625;

        public long Version = 0;

        public bool LogEnabled = false;

        public bool InitializeOnAwake = true;

        public bool ConnectOnAwake = false;

        public static ClientOptions<T> SocketOptions = new ClientOptions<T>
        {
            AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork,
            ProtocolType = System.Net.Sockets.ProtocolType.Tcp,
            ReceiveBufferSize = 81960,
            inputCipher = new SCL.Cipher.PacketNoneCipher(),
            outputCipher = new SCL.Cipher.PacketNoneCipher(),
        };

        protected virtual void Awake()
        {
            if (InitializeOnAwake)
                StartClient();
        }

        public void StartClient()
        {
            if (SocketOptions.NetworkClient != null)
            {
                if (!SocketOptions.NetworkClient.GetState())
                {
                    SocketOptions.NetworkClient.Connect();
                }
                return;
            }

            LoadPackets();

            SocketOptions.IpAddress = LocalServer ? "127.0.0.1" : (TimkaServer ? TimkaIpAddress : IpAddress);
            SocketOptions.Port = Port;

            SocketOptions.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            SocketOptions.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            SocketOptions.OnExtensionEvent += SocketOptions_OnExtensionEvent;
            SocketOptions.NetworkClient = new SocketClient<T>(SocketOptions);
            SocketOptions.NetworkClient.Version = Version;
#if DEBUG
            SocketOptions.NetworkClient.OnReceivePacket += NetworkClient_OnReceivePacket;
            SocketOptions.NetworkClient.OnSendPacket += NetworkClient_OnSendPacket;
#endif
            if(ConnectOnAwake)
                SocketOptions.NetworkClient.Connect();
        }

#if DEBUG

        protected virtual void NetworkClient_OnSendPacket(Client<T> client, ushort pid, int len)
        {
            if(LogEnabled)
                Debug.Log($"{ClientType} packet send pid:{pid} len:{len}");
        }

        protected virtual void NetworkClient_OnReceivePacket(Client<T> client, ushort pid, int len)
        {
            if (LogEnabled)
                Debug.Log($"{ClientType} packet receive pid:{pid} len:{len}");
        }

#endif

        protected virtual void LoadPackets()
        {
        }

        protected virtual void SocketOptions_OnExtensionEvent(Exception ex, T client)
        {
            Debug.Log(ex.ToString());
        }

        protected virtual void SocketOptions_OnClientConnectEvent(T client)
        {
            Debug.Log($"{ClientType} server connected");
        }

        protected virtual void SocketOptions_OnClientDisconnectEvent(T client)
        {
            Debug.LogError($"{ClientType} server connection lost");
        }

        protected virtual void OnApplicationQuit()
        {
            if (SocketOptions?.NetworkClient != null)
            {
                SocketOptions.NetworkClient.Disconnect();
                SocketOptions.NetworkClient = null;
            }
        }

        public static event ClientOptions<T>.ExtensionHandleDelegate OnExtensionEvent
        {
            add { SocketOptions.OnExtensionEvent += value; }
            remove { SocketOptions.OnExtensionEvent -= value; }
        }

        public static event ClientOptions<T>.ClientConnectedDelegate OnClientConnectEvent
        {
            add { SocketOptions.OnClientConnectEvent += value; }
            remove { SocketOptions.OnClientConnectEvent -= value; }
        }

        public static event ClientOptions<T>.ClientDisconnectedDelegate OnClientDisconnectEvent
        {
            add { SocketOptions.OnClientDisconnectEvent += value; }
            remove { SocketOptions.OnClientDisconnectEvent -= value; }
        }

        public static event ClientOptions<T>.RecoverySessionDelegate OnRecoverySessionEvent
        {
            add { SocketOptions.OnRecoverySessionEvent += value; }
            remove { SocketOptions.OnRecoverySessionEvent -= value; }
        }

        public static bool Connect()
        {
            return SocketOptions.NetworkClient.Connect();
        }

        public static async Task<bool> ConnectAsync(string ip, int port)
        {
            SocketOptions.IpAddress = ip;
            SocketOptions.Port = port;
            return await SocketOptions.NetworkClient.ConnectAsync();
        }

        public static async Task<bool> ConnectAsync()
        {
            return await SocketOptions.NetworkClient.ConnectAsync();
        }

        public static bool GetConnectionState()
        {
            return SocketOptions.NetworkClient?.GetState() ?? false;
        }

        public static void Disconnect(bool disconnectEventCall = true)
        {
            if (GetConnectionState())
                SocketOptions.NetworkClient.Disconnect(disconnectEventCall);
        }
    }
}
