using NSL.SocketClient.Node.Utils;
using UnityEngine;

namespace NSL.SocketClient.Node.Unity
{
    public static class UnityBufferExtensions
    {
        public static Vector2 ReadVector2(this NodeInputPacketBuffer packet)
        {
            return new Vector2(packet.ReadFloat(), packet.ReadFloat());
        }

        public static Vector3 ReadVector3(this NodeInputPacketBuffer packet)
        {
            return new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
        }

        public static void WriteVector2(this NodeOutputPacketBuffer packet, Vector2 value)
        {
            packet.WriteFloat32(value.x);
            packet.WriteFloat32(value.y);
        }

        public static void WriteVector3(this NodeOutputPacketBuffer packet, Vector3 value)
        {
            packet.WriteFloat32(value.x);
            packet.WriteFloat32(value.y);
            packet.WriteFloat32(value.z);
        }
    }
}
