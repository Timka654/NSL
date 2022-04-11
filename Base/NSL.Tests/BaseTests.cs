using NUnit.Framework;
using SocketClient;
using SocketCore.Utils.Buffer;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Tests
{
    internal class BaseTests
    {
        private TestClient _client;

        private TestServer _server;

        [SetUp]
        public void Startup()
        {
            _client = new TestClient();
            _server = new TestServer();
        }

        [Test]
        public async Task TestServerRun()
        {
            _server.Server.Run();

            Debug.Assert(_server.Server.State);
        }

        [Test]
        public async Task TestConnection()
        {
            await TestServerRun();

            Debug.Assert(_client.Client.Connect());
        }

        [Test]
        public async Task TestConnected()
        {
            int exCount = 0;

            bool connected = false;

            bool serverConnected = false;

            _server.serverOptions.OnClientConnectEvent += (client) =>
            {
                serverConnected = true;
            };

            _client.clientOptions.OnClientConnectEvent += (client) =>
            {
                connected = true;

            };

            _client.clientOptions.OnExceptionEvent += (ex, client) =>
            {
                exCount++;

            };
            await TestConnection();

            await Task.Delay(3000);

            Debug.Assert(exCount == 0);

            Debug.Assert(serverConnected);
            Debug.Assert(connected);

            Debug.Assert(_client.Client.ConnectionOptions.ClientData.GetState());

            Debug.Assert(_client.Client.ConnectionOptions.ClientData.Network.GetState());
        }

        [Test]
        public async Task TestClientDisconnect()
        {
            int exCount = 0;

            bool disconnected = false;
            _client.clientOptions.OnClientDisconnectEvent += (client) =>
            {
                disconnected = true;

            };

            _client.clientOptions.OnExceptionEvent += (ex, client) =>
            {
                exCount++;

            };
            await TestConnection();

            _client.Client.Disconnect();

            await Task.Delay(150);

            Debug.Assert(exCount == 1);

            Debug.Assert(disconnected);

            Debug.Assert(!_client.Client.ConnectionOptions.ClientData.GetState());

            Debug.Assert(!_client.Client.ConnectionOptions.ClientData.Network.GetState());
        }

        [Test]
        public async Task TestServerDisconnect()
        {
            int exCount = 0;

            bool disconnected = false;
            _client.clientOptions.OnClientDisconnectEvent += (client) =>
            {
                disconnected = true;

            };

            _client.clientOptions.OnExceptionEvent += (ex, client) =>
            {
                exCount++;

            };

            SocketServerNetworkClient serverClient = default;

            _server.serverOptions.OnClientConnectEvent += client => serverClient = client;

            await TestConnection();

            await Task.Delay(150);

            serverClient?.Network.Disconnect();

            await Task.Delay(150);

            Debug.Assert(exCount == 1);

            Debug.Assert(disconnected);

            Debug.Assert(!_client.Client.ConnectionOptions.ClientData.GetState());

            Debug.Assert(!_client.Client.ConnectionOptions.ClientData.Network.GetState());
        }

        [Test]
        public async Task TestServerInvalidSend()
        {
            bool serverConnected = false;

            bool serverDisconnected = false;

            _server.serverOptions.OnClientConnectEvent += (client) =>
            {
                serverConnected = true;
                var packet = new OutputPacketBuffer();
                packet.PacketId = 1;
                client.Send(packet);
            };

            _server.serverOptions.OnClientDisconnectEvent += (client) =>
            {
                serverDisconnected = true;
            };

            int serverExCount = 0;
            _server.serverOptions.OnExceptionEvent += (ex, client) =>
            {
                serverExCount++;
            };

            int exCount = 0;

            _client.clientOptions.OnExceptionEvent += (ex, client) =>
            {
                exCount++;
            };

            bool clientDisconnected = false;

            _client.clientOptions.OnClientDisconnectEvent += (client) =>
            {
                clientDisconnected = true;
            };

            await TestConnection();

            await Task.Delay(150);

            Debug.Assert(serverConnected);

            Debug.Assert(!serverDisconnected);

            Debug.Assert(serverExCount == 0);

            Debug.Assert(exCount == 1);

            Debug.Assert(!clientDisconnected);
        }

        [Test]
        public async Task TestClientInvalidSend()
        {
            bool serverConnected = false;

            bool serverDisconnected = false;

            _server.serverOptions.OnClientConnectEvent += (client) =>
            {
                serverConnected = true;
            };

            _server.serverOptions.OnClientDisconnectEvent += (client) =>
            {
                serverDisconnected = true;
            };

            int serverExCount = 0;
            _server.serverOptions.OnExceptionEvent += (ex, client) =>
            {
                serverExCount++;
            };

            int exCount = 0;

            _client.clientOptions.OnExceptionEvent += (ex, client) =>
            {
                exCount++;
            };

            bool clientDisconnected = false;
            bool clientConnected = false;

            _client.clientOptions.OnClientConnectEvent += (client) =>
            {
                clientConnected = true;
                var packet = new OutputPacketBuffer();
                packet.PacketId = 1;
                client.Send(packet);
            };

            _client.clientOptions.OnClientDisconnectEvent += (client) =>
            {
                clientDisconnected = true;
            };

            await TestConnection();

            await Task.Delay(150);

            Debug.Assert(serverConnected);

            Debug.Assert(clientConnected);

            Debug.Assert(!serverDisconnected);

            Debug.Assert(serverExCount == 1);

            Debug.Assert(exCount == 0);

            Debug.Assert(!clientDisconnected);
        }

        [Test]
        public async Task TestAliveChecker()
        {
            _client.clientOptions.OnClientConnectEvent += (client) =>
            {

                client.PingPongEnabled = true;
            };

            SocketServerNetworkClient serverClient = default;

            _server.serverOptions.OnClientConnectEvent += (client) => serverClient = client;

            await TestConnection();

            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }
}
