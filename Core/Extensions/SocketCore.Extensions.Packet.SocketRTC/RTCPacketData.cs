using System.Collections.Generic;

namespace NSL.SocketCore.Extensions.Packet.SocketRTC
{
    class RTCPacketData
    {
        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public List<object> Args { get; set; }
    }
}
