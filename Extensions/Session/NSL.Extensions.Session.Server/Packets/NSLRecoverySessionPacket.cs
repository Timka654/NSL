using NSL.Extensions.Session;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Server.Packets
{
    public class NSLRecoverySessionPacket<T> : IAsyncPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 2;

        public override async Task ReceiveAsync(T client, InputPacketBuffer data)
        {
            var pid = client.ServerOptions.ObjectBag.Get<ushort>(RequestProcessor.DefaultResponsePIDObjectBagKey, true);

            var response = data.CreateResponse(pid);

            var request = NSLSessionInfo.ReadFullFrom(data);

            if (request == null)
                return;

            client.ThrowIfObjectBagNull();

            var serverOptions = client.ServerOptions;

            var sessionManager = serverOptions.ObjectBag.Get<NSLSessionManager<T>>(NSLSessionManager<T>.ObjectBagKey);

            var result = await sessionManager.TryRecovery(client, request.Session, request.RestoreKeys);

            result.WriteFullTo(response);

            var nc = ((NSLServerSessionInfo<T>)result.SessionInfo)?.Client ?? client;

            nc.Network?.Send(response);
        }
    }
}
