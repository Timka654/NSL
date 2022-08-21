using NSL.SocketCore;
using System.Threading.Tasks;

namespace NSL.SocketClient.Unity
{
    public class UnityBaseNetworkStatic<T> : UnityBaseNetwork<T>
        where T : BaseSocketNetworkClient, new()
    {
        public static UnityBaseNetworkStatic<T> Instance { get; private set; }


        public new static UnityClientOptions<T> SocketOptions => ((UnityBaseNetwork<T>)Instance).SocketOptions;

        public new static UnityTCPClient<T> NetworkClient => ((UnityBaseNetwork<T>)Instance).NetworkClient;


        public new static event CoreOptions<T>.ExceptionHandle OnExtensionEvent
        {
            add { SocketOptions.OnExceptionEvent += value; }
            remove { SocketOptions.OnExceptionEvent -= value; }
        }

        public new static event CoreOptions<T>.ClientConnect OnClientConnectEvent
        {
            add { SocketOptions.OnClientConnectEvent += value; }
            remove { SocketOptions.OnClientConnectEvent -= value; }
        }

        public new static event CoreOptions<T>.ClientDisconnect OnClientDisconnectEvent
        {
            add { SocketOptions.OnClientDisconnectEvent += value; }
            remove { SocketOptions.OnClientDisconnectEvent -= value; }
        }

        public new static event UnityClientOptions<T>.ReconnectDelegate OnRecoverySessionEvent
        {
            add { SocketOptions.OnReconnectEvent += value; }
            remove { SocketOptions.OnReconnectEvent -= value; }
        }

        public new static bool Connect()
        {
            return NetworkClient.Connect();
        }

        public new static async Task<bool> ConnectAsync()
        {
            return await NetworkClient.ConnectAsync();
        }

        public new static async Task<bool> ConnectAsync(string ip, int port)
        {
            return await NetworkClient.ConnectAsync(ip, port);
        }

        public new static bool GetConnectionState()
        {
            return SocketOptions.NetworkClient?.GetState() ?? false;
        }

        public new static void Disconnect(bool disconnectEventCall = true)
        {
            if (GetConnectionState())
                NetworkClient.Disconnect();
        }
    }
}
