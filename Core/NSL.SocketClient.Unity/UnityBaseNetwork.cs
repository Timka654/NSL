using NSL.TCP.Client;
using NSL.SocketCore;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace NSL.SocketClient.Unity
{
    public class UnityBaseNetwork<T> : MonoBehaviour
        where T : BaseSocketNetworkClient
    {
        protected virtual string ClientType { get; }

        public bool Enabled = true;

        public bool LocalServer = false;

        public string IpAddress = "5.53.116.170";

        public int Port = 4625;

        public long Version = 0;

        public bool LogEnabled = false;

        public bool InitializeOnAwake = true;

        public bool ConnectOnAwake = false;

        public UnityClientOptions<T> SocketOptions { get; private set; } = new UnityClientOptions<T>
        {
            ReceiveBufferSize = 81960
        };

        public UnityTCPClient<T> NetworkClient { get; private set; }

        protected virtual async void Awake()
        {
            if (InitializeOnAwake)
                await InitializeClient(ConnectOnAwake);
        }

        public async Task InitializeClient(bool connect = false)
        {
            if (NetworkClient != null)
            {
                if (!NetworkClient.GetState() && connect)
                    NetworkClient.Connect();

                return;
            }

            SocketOptions.IpAddress = LocalServer ? "127.0.0.1" : IpAddress;
            SocketOptions.Port = Port;

            SocketOptions.OnClientConnectEvent += SocketOptions_OnClientConnectEvent;
            SocketOptions.OnClientDisconnectEvent += SocketOptions_OnClientDisconnectEvent;
            SocketOptions.OnExceptionEvent += SocketOptions_OnExtensionEvent;

            LoadOptions();

            NetworkClient = new UnityTCPClient<T>(SocketOptions);

            NetworkClient.Version = Version;

            NetworkClient.OnReceivePacket += NetworkClient_OnReceivePacket;
            NetworkClient.OnSendPacket += NetworkClient_OnSendPacket;

            if (connect)
            {
                await ConnectAsync();
            }
        }

        protected virtual void NetworkClient_OnSendPacket(TCPClient<T> client, ushort pid, int len, string stackTrace)
        {
            if (LogEnabled)
                Debug.Log($"{ClientType} packet send pid:{pid} len:{len}");
        }

        protected virtual void NetworkClient_OnReceivePacket(TCPClient<T> client, ushort pid, int len)
        {
            if (LogEnabled)
                Debug.Log($"{ClientType} packet receive pid:{pid} len:{len}");
        }

        protected virtual void LoadOptions()
        {
        }

        protected virtual void SocketOptions_OnExtensionEvent(Exception ex, T client)
        {
            Debug.LogError(ex.ToString());
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
                SocketOptions.NetworkClient.Disconnect();
        }

        public event CoreOptions<T>.ExceptionHandle OnExtensionEvent
        {
            add { SocketOptions.OnExceptionEvent += value; }
            remove { SocketOptions.OnExceptionEvent -= value; }
        }

        public event CoreOptions<T>.ClientConnect OnClientConnectEvent
        {
            add { SocketOptions.OnClientConnectEvent += value; }
            remove { SocketOptions.OnClientConnectEvent -= value; }
        }

        public event CoreOptions<T>.ClientDisconnect OnClientDisconnectEvent
        {
            add { SocketOptions.OnClientDisconnectEvent += value; }
            remove { SocketOptions.OnClientDisconnectEvent -= value; }
        }

        public event UnityClientOptions<T>.ReconnectDelegate OnRecoverySessionEvent
        {
            add { SocketOptions.OnReconnectEvent += value; }
            remove { SocketOptions.OnReconnectEvent -= value; }
        }

        public bool Connect()
        {
            return NetworkClient.Connect();
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            SocketOptions.IpAddress = ip;
            SocketOptions.Port = port;
            return await NetworkClient.ConnectAsync();
        }

        public async Task<bool> ConnectAsync()
        {
            return await NetworkClient.ConnectAsync();
        }

        public bool GetConnectionState()
        {
            return SocketOptions.NetworkClient?.GetState() ?? false;
        }

        public void Disconnect(bool disconnectEventCall = true)
        {
            if (GetConnectionState())
                NetworkClient.Disconnect();
        }
    }
}
