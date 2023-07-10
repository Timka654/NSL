using NSL.Extensions.Session;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;
using System.Linq;

namespace NSL.Extensions.Session.Server.Packets
{
    public class RecoverySessionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public override void Receive(T client, InputPacketBuffer data)
        {
            var response = data.CreateResponse(client.ServerOptions.);

            var request = NSLSessionInfo.ReadFullFrom(data);

            if (request == null)
                return;

            client.ThrowIfObjectBagNull();

            var serverOptions = client.ServerOptions as ServerOptions<T>;

            var sessionManager = serverOptions.ObjectBag.Get<NSLSessionManager<T>>(NSLSessionManager<T>.ObjectBagKey);

            var result = sessionManager.TryRecovery(client, request.Session, request.RestoreKeys);

            result.WriteFullTo(response);

            client.Network?.Send(response);
        }
    }
}
