using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Session;
using NSL.SocketServer.Utils;
using System.Threading.Tasks;

namespace NSL.SocketServer.Utils.Session
{
    public class NSLRecoverySessionPacket<T> : IAsyncPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = (ushort)NSLSystemPacketEnum.Session;

        public override async Task ReceiveAsync(T client, InputPacketBuffer data)
        {
            var pid = client.ServerOptions.ObjectBag.Get<ushort>(NSLObjectBagKeys.ResponsePID, true);

            var response = data.CreateResponse(pid);

            var request = NSLSessionInfo.ReadFullFrom(data);

            if (request == null)
                return;

            client.ThrowIfObjectBagNull();

            var serverOptions  = client.ServerOptions;
            var sessionManager = serverOptions.ObjectBag.Get<NSLSessionManager<T>>(NSLSessionManager<T>.ObjectBagKey);
            var result         = await sessionManager.TryRecovery(client, request.Session, request.RestoreKeys);

            result.WriteFullTo(response);

            var nc = ((NSLServerSessionInfo<T>)result.SessionInfo)?.Client ?? client;
            nc.Network?.Send(response);
        }
    }
}
