using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.Packet.SocketRTC
{
    class RTCPacketData
    {
        public string ClassName { get; set; }

        public string MethodName { get; set; }

        public List<object> Args { get; set; }
    }
}
