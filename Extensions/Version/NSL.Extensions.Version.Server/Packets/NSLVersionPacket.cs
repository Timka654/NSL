using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer;
using NSL.SocketServer.Utils;

namespace NSL.Extensions.Version.Server.Packets
{
    public class NSLVersionPacket<T> : IPacket<T> where T : IServerNetworkClient
    {
        public const ushort PacketId = ushort.MaxValue - 3;

        public override void Receive(T client, InputPacketBuffer data)
        {
            var pid = client.ServerOptions.ObjectBag.Get<ushort>(RequestProcessor.DefaultResponsePIDObjectBagKey, true);

            var response = data.CreateResponse(pid);

            var request = NSLVersionInfo.ReadFullFrom(data);

            var serverVersion = client.ServerOptions.ObjectBag.Get<NSLServerVersionInfo>(NSLServerVersionInfo.ObjectBagKey);

            new NSLVersionResult
            {
                Version = serverVersion.Version,
                MinVersion = serverVersion.MinVersion,
                RequireVersion = serverVersion.RequireVersion,
                InvalidByMinVersion = !serverVersion.ValidateMinVersion(request.Version),
                InvalidByReqVersion = !serverVersion.ValidateRequireVersion(request.Version)
            }.WriteResponseTo(response);

            client.Send(response);
        }
    }
}
