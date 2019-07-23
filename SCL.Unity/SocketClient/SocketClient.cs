using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SCL.SocketClient
{
    public class SocketClient<T> : Client<T>
        where T : BaseSocketNetworkClient
    {
        Socket client;

        public SocketClient(ClientOptions<T> options) : base(options)
        {
        }

        public bool Connect()
        {
            try
            {
                client = new Socket(clientOptions.AddressFamily, SocketType.Stream, clientOptions.ProtocolType);
                client.Connect(clientOptions.IpAddress, clientOptions.Port);

                Reconnect(client);
                return true;
            }
            catch (Exception ex)
            {
                clientOptions.RunExtension(ex);
                clientOptions.RunClientDisconnect();
            }
            return false;
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            this.clientOptions.IpAddress = ip;
            this.clientOptions.Port = port;
            return await ConnectAsync();
        }

        public async Task<bool> ConnectAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    client = new Socket(clientOptions.AddressFamily, SocketType.Stream, clientOptions.ProtocolType);
                    //await client.ConnectAsync(clientOptions.IpAddress, clientOptions.Port);

                    IAsyncResult result = client.BeginConnect(clientOptions.IpAddress, clientOptions.Port, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                    if (success && client.Connected)
                        Reconnect(client);
                    return success;
                }
                catch (Exception ex)
                {
                    clientOptions.RunExtension(ex);
                    clientOptions.RunClientDisconnect();
                }

                return false;
            });
        }
    }
}
