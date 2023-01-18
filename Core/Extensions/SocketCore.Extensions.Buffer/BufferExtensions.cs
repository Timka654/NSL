using Newtonsoft.Json;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Net.Sockets;

namespace NSL.SocketCore.Extensions.Buffer
{
    public static class BufferExtensions
    {
        /// <summary>
        /// Read string 16 bit len value and deserialize to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static T ReadJson16<T>(this InputPacketBuffer packet)
        {
            return JsonConvert.DeserializeObject<T>(packet.ReadString16());
        }

        public static object ReadJson16(this InputPacketBuffer packet, Type type)
        {
            return JsonConvert.DeserializeObject(packet.ReadString16(), type);
        }

        /// <summary>
        /// Read string 32 bit len value and deserialize to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static T ReadJson32<T>(this InputPacketBuffer packet)
        {
            return JsonConvert.DeserializeObject<T>(packet.ReadString32());
        }

        public static object ReadJson32(this InputPacketBuffer packet, Type type)
        {
            return JsonConvert.DeserializeObject(packet.ReadString32(), type);
        }

        /// <summary>
        /// Write serializer json string 16 bit len value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void WriteJson16<T>(this OutputPacketBuffer buffer, T value)
        {
            buffer.WriteString16(JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Write serializer json string 32 bit len value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="value"></param>
        public static void WriteJson32<T>(this OutputPacketBuffer buffer, T value)
        {
            buffer.WriteString32(JsonConvert.SerializeObject(value));
        }

        public static void SendJson16<TClient, TObject>(this TClient client, ushort packetId, TObject o)
            where TClient : IClient
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            packet.WriteJson16(o);

            client.Send(packet);
        }

        public static void SendJson16<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o)
            where TClient : IClient
            where TPacket : Enum
        {
            SendJson16(client, Convert.ToUInt16(packetId), o);
        }

        public static void SendJson16<TClient, TPacket>(this TClient client, TPacket packetId, object o)
            where TClient : IClient
            where TPacket : Enum
        {
            SendJson16(client, Convert.ToUInt16(packetId), o);
        }

        public static void SendJson32<TClient, TObject>(this TClient client, ushort packetId, TObject o)
            where TClient : IClient
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            packet.WriteJson32(o);

            client.Send(packet);
        }

        public static void SendJson32<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            SendJson32(client, Convert.ToUInt16(packetId), o);
        }

        public static void SendJson32<TClient, TPacket>(this TClient client, TPacket packetId, object o)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            SendJson32(client, Convert.ToUInt16(packetId), o);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, int value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, byte value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteByte(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, bool value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteBool(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, short value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, ushort value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, uint value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteUInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, long value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteInt64(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, ulong value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteUInt64(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, float value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteFloat(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, double value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteDouble(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, DateTime value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteDateTime(value);

            client.Send(rbuff);
        }

        public static void Send<TClient, TPacket>(this TClient client, TPacket packetId, string value)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            var rbuff = OutputPacketBuffer.Create(packetId);

            rbuff.WriteString16(value);

            client.Send(rbuff);
        }

        public static void SendEmpty<TClient, TPacket>(this TClient client, TPacket packetId)
            where TClient : IClient
            where TPacket : struct, Enum, IConvertible
        {
            client.Send(OutputPacketBuffer.Create(packetId));
        }

        /// <summary>
        /// Create response for wait request
        /// Warning!! Input packet have identity - 16 first byte or request, execute this method before read any data
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static OutputPacketBuffer CreateWaitBufferResponse<TClient>(this TClient client, InputPacketBuffer data)
            where TClient : IClient
        {
            return data.CreateWaitBufferResponse();
        }

        /// <summary>
        /// Create response for wait request
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static OutputPacketBuffer CreateWaitBufferResponse(this InputPacketBuffer data)
            => new OutputPacketBuffer()
                .WithWaitableAnswer(data);

        public static OutputPacketBuffer WithWaitableAnswer(this OutputPacketBuffer buffer, InputPacketBuffer data)
        {
            var boffset = buffer.Position;

            var doffset = data.Position;

            data.DataPosition = 0;

            buffer.DataPosition = 0;

            buffer.WriteGuid(data.ReadGuid());

            if (boffset > WaitablePacketBuffer.DefaultHeaderLenght)
                buffer.Position = boffset;
            else
                buffer.DataPosition = 16;

            if (doffset > WaitablePacketBuffer.DefaultHeaderLenght)
                data.Position = doffset;
            else
                data.DataPosition = 16;

            return buffer;
        }

    }
}
