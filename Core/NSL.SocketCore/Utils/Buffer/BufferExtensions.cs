using Newtonsoft.Json;
using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Utils.Buffer
{
    public static class BufferExtensions
    {
        public static T ReadJson16<T>(this InputPacketBuffer packet)
            => JsonConvert.DeserializeObject<T>(packet.ReadString());

        public static object ReadJson16(this InputPacketBuffer packet, Type type)
            => JsonConvert.DeserializeObject(packet.ReadString(), type);

        public static T ReadJson32<T>(this InputPacketBuffer packet)
            => JsonConvert.DeserializeObject<T>(packet.ReadString());

        public static object ReadJson32(this InputPacketBuffer packet, Type type)
            => JsonConvert.DeserializeObject(packet.ReadString(), type);

        public static void WriteJson16<T>(this OutputPacketBuffer buffer, T value)
            => buffer.WriteString(JsonConvert.SerializeObject(value));

        public static void WriteJson32<T>(this OutputPacketBuffer buffer, T value)
            => buffer.WriteString(JsonConvert.SerializeObject(value));

        public static void SendJson16<TClient, TObject>(this TClient client, ushort packetId, TObject o)
            where TClient : IClient
        {
            var packet = new OutputPacketBuffer { PacketId = packetId };
            packet.WriteJson16(o);
            client.Send(packet);
        }

        public static void SendJson16<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o)
            where TClient : IClient
            where TPacket : Enum
            => SendJson16(client, Convert.ToUInt16(packetId), o);

        public static void SendJson16<TClient, TPacket>(this TClient client, TPacket packetId, object o)
            where TClient : IClient
            where TPacket : Enum
            => SendJson16(client, Convert.ToUInt16(packetId), o);

        public static void SendJson32<TClient, TObject>(this TClient client, ushort packetId, TObject o)
            where TClient : IClient
        {
            var packet = new OutputPacketBuffer { PacketId = packetId };
            packet.WriteJson32(o);
            client.Send(packet);
        }

        public static void SendJson32<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
            => SendJson32(client, Convert.ToUInt16(packetId), o);

        public static void SendJson32<TClient, TPacket>(this TClient client, TPacket packetId, object o)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
            => SendJson32(client, Convert.ToUInt16(packetId), o);

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, int value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteInt32(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, byte value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteByte(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, bool value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteBool(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, short value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteInt32(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, ushort value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteInt32(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, uint value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteUInt32(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, long value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteInt64(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, ulong value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteUInt64(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, float value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteFloat(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, double value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteDouble(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, DateTime value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteDateTime(value); client.Send(b); }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, string value)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
        { var b = OutputPacketBuffer.Create(packetId); b.WriteString(value); client.Send(b); }

        public static void SendEmpty<TClient, TPacket>(this TClient client, TPacket packetId)
            where TClient : IClient where TPacket : struct, Enum, IConvertible
            => client.Send(OutputPacketBuffer.Create(packetId));

        public static OutputPacketBuffer CreateWaitBufferResponse<TClient>(this TClient client, InputPacketBuffer data)
            where TClient : IClient
            => data.CreateWaitBufferResponse();

        public static OutputPacketBuffer CreateWaitBufferResponse(this InputPacketBuffer data)
            => new OutputPacketBuffer().WithWaitableAnswer(data);

        public static OutputPacketBuffer WithWaitableAnswer(this OutputPacketBuffer buffer, InputPacketBuffer data)
        {
            if (buffer.Length < 23)
                buffer.SetLength(23);

            data.Data.AsSpan(0, 16)
                .CopyTo(new ArraySegment<byte>(buffer.GetBuffer(), OutputPacketBuffer.DefaultHeaderLength, 16));

            if (buffer.DataPosition < 16)
                buffer.DataPosition = 16;

            if (data.DataPosition < 16)
                data.DataPosition = 16;

            return buffer;
        }
    }
}
