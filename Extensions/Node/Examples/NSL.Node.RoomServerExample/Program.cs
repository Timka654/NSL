using NSL.Logger;
using NSL.Node.BridgeServer.Shared;
using NSL.Node.RoomServer;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Client.Data;
using NSL.Node.RoomServer.Data;
using NSL.SocketCore.Utils.Exceptions;

namespace NSL.Node.BridgeTransportExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var x = new RoomConfigurationManager(null);

            //foreach (var r in x.GetAllValues())
            //{
            //    Console.WriteLine($"{r.Path}  ::  {r.Value}");
            //}

            //ExampleRoomServerStartupEntry.CreateDefault().RunEntry();

            int clientPort = 9999;

            var roomServerLogger = new ConsoleLogger();

            NodeRoomServerEntryBuilder.Create()
                .WithLogger(roomServerLogger)
                .WithBridgeDefaultHandles()
                //.WithCreateSessionHandle(roomInfo=> new GameInfo(roomInfo))
                //.GetPublicAddressFromStun(clientPort,false, out var connectionPoint)
                .WithRoomBridgeNetwork("wss://localhost:7023/room_server", new Dictionary<string, string>(), "tcp://localhost:9999", NodeNetworkHandles<BridgeRoomNetworkClient>.Create()
                .WithExceptionHandle((ex, client) => {
                    if ((ex is ConnectionLostException cle && cle.InnerException == null)
                    || ex is TaskCanceledException)
                        return;

                    roomServerLogger?.AppendError($"Client {client.Network?.GetRemotePoint()} have error - {ex.ToString()}");
                })
                .WithConnectHandle((client) => {

                    roomServerLogger.AppendInfo($"Client {client.Network?.GetRemotePoint()} connected");

                    return Task.CompletedTask;
                })
                .WithDisconnectHandle((client) => {

                    roomServerLogger.AppendInfo($"Client {client.Network?.GetRemotePoint()} disconnected");

                    return Task.CompletedTask;
                })
                .WithSendHandle((client, pid, len, stack) => {
                    roomServerLogger.AppendInfo($"Send packet {pid} to {client.Network?.GetRemotePoint()}");
                })
                .WithReceiveHandle((client, pid, len) => {
                    roomServerLogger.AppendInfo($"Receive packet {pid} from {client.Network?.GetRemotePoint()}");
                }))
                .WithTCPClientServerBinding(clientPort
                , NodeNetworkHandles<TransportNetworkClient>.Create()
                    .WithExceptionHandle((ex, client) => {
                        if (ex is ConnectionLostException cle && cle.InnerException == null)
                            return;

                        roomServerLogger?.AppendError($"Client {client.Network?.GetRemotePoint()} have error - {ex.ToString()}");
                    })
                    .WithConnectHandle((client) => {

                        roomServerLogger.AppendInfo($"Client {client.Network?.GetRemotePoint()} connected");

                        return Task.CompletedTask;
                    })
                    .WithDisconnectHandle((client) => {

                        roomServerLogger.AppendInfo($"Client {client.Network?.GetRemotePoint()} disconnected");

                        return Task.CompletedTask;
                    })
                    .WithSendHandle((client, pid, len, stack) => {
                        roomServerLogger.AppendInfo($"Send packet {pid} to {client.Network?.GetRemotePoint()}");
                    })
                    .WithReceiveHandle((client, pid, len) => {
                        roomServerLogger.AppendInfo($"Receive packet {pid} from {client.Network?.GetRemotePoint()}");
                    })
                ).Run();


            Console.WriteLine("Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}
