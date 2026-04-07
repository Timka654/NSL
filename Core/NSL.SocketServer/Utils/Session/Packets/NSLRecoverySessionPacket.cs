using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.SocketCore.Network.Session;
using System.Threading.Tasks;

namespace NSL.SocketServer.Utils.Session.Packets
{
    public class NSLRecoverySessionPacket<T> : IAsyncPacket<T> where T : BaseNetworkConnection, new()
    {
        public const ushort PacketId = (ushort)NSLSystemPacketEnum.Session;

        public override async Task ReceiveAsync(T client, InputPacketBuffer data)
        {
            var pid = client.Options.ObjectBag.Get<ushort>(NSLObjectBagKeys.ResponsePID, true);

            var response = data.CreateResponse(pid);

            var request = NSLSessionInfo.ReadFullFrom(data);

            if (request == null)
                return;

            client.ThrowIfObjectBagNull();

            var serverOptions  = client.Options;
            var sessionManager = serverOptions.ObjectBag.Get<NSLSessionManager<T>>(NSLSessionManager<T>.ObjectBagKey);
            var result         = await sessionManager.TryRecovery(client, request.Session, request.RestoreKeys);

            result.WriteFullTo(response);

            var nc = ((NSLServerSessionInfo<T>)result.SessionInfo)?.Client ?? client;
            nc.Network?.Send(response);
        }
    }
}
