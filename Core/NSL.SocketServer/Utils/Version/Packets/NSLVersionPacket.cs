using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketCore.Utils.Request;
using NSL.SocketCore.Network.Version;

namespace NSL.SocketServer.Utils.Version.Packets
{
    public class NSLVersionPacketReceive<T> : IPacket<T> where T : BaseNetworkConnection
    {
        public const ushort PacketId = (ushort)NSLSystemPacketEnum.Version;

        public override void Receive(T client, InputPacketBuffer data)
        {
            var pid = client.Options.ObjectBag.Get<ushort>(NSLObjectBagKeys.ResponsePID, true);

            var response = data.CreateResponse(pid);

            var request = NSLVersionInfo.ReadFullFrom(data);

            var serverVersion = client.Options.ObjectBag.Get<NSLServerVersionInfo>(NSLVersionInfo.ObjectBagKey);

            new NSLVersionResult
            {
                Version             = serverVersion.Version,
                MinVersion          = serverVersion.MinVersion,
                RequireVersion      = serverVersion.RequireVersion,
                InvalidByMinVersion = !serverVersion.ValidateMinVersion(request.Version),
                InvalidByReqVersion = !serverVersion.ValidateRequireVersion(request.Version)
            }.WriteResponseTo(response);

            client.Send(response);
        }
    }
}
