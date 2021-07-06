using Newtonsoft.Json;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using System;
using System.Reflection.Metadata;

namespace SocketCore.Extensions.Buffer
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

        public static void SendJson16<TClient, TObject>(this TClient client, ushort packetId, TObject o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
            where TClient : INetworkClient
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            packet.WriteJson16(o);

            client.Network.Send(packet);
        }

        public static void SendJson16<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
    where TClient : INetworkClient
            where TPacket : Enum
        {
            SendJson16<TClient, TObject>(client, Convert.ToUInt16(packetId), o
#if DEBUG
                , memberName,
                sourceFilePath,
                sourceLineNumber
#endif
                );
        }

        public static void SendJson16<TClient, TPacket>(this TClient client, TPacket packetId, object o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
    where TClient : INetworkClient
            where TPacket : Enum
        {
            SendJson16<TClient, object>(client, Convert.ToUInt16(packetId), o
#if DEBUG
                , memberName,
                sourceFilePath,
                sourceLineNumber
#endif
                );
        }

        public static void SendJson32<TClient, TObject>(this TClient client, ushort packetId, TObject o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
            where TClient : INetworkClient
        {
            var packet = new OutputPacketBuffer();

            packet.PacketId = packetId;

            packet.WriteJson32(o);

            client.Network.Send(packet);
        }

        public static void SendJson32<TClient, TPacket, TObject>(this TClient client, TPacket packetId, TObject o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
    where TClient : INetworkClient
            where TPacket : Enum
        {
            SendJson32<TClient, TObject>(client, Convert.ToUInt16(packetId), o
#if DEBUG
                , memberName,
                sourceFilePath,
                sourceLineNumber
#endif
                );
        }

        public static void SendJson32<TClient, TPacket>(this TClient client, TPacket packetId, object o
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
    where TClient : INetworkClient
            where TPacket : Enum
        {
            SendJson32<TClient, object>(client, Convert.ToUInt16(packetId), o
#if DEBUG
                , memberName,
                sourceFilePath,
                sourceLineNumber
#endif
                );
        }



        public static void Send<TClient>(this TClient client, ushort packetId, int value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);
            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, byte value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteByte(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, bool value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteBool(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, short value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, ushort value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, uint value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteUInt32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, long value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteInt64(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, ulong value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteUInt64(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, float value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteFloat32(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, double value
#if DEBUG
            , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
#endif
            )
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteFloat64(value);

            client.Send(rbuff.CompilePacket(), 0, rbuff.PacketLenght);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, DateTime value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteDateTime(value);

            client.Send(rbuff);
        }

        public static void Send<TClient>(this TClient client, ushort packetId, string value)
    where TClient : IClient
        {
            OutputPacketBuffer rbuff = new OutputPacketBuffer
            {
                PacketId = packetId
            };

            rbuff.WriteString16(value);

            client.Send(rbuff);
        }


    }

    public class OutputPacketBuffer<TPacketId> : OutputPacketBuffer
        where TPacketId : struct, Enum, IConvertible
    {
        public new TPacketId PacketId { get => (TPacketId)Enum.ToObject(typeof(TPacketId), base.PacketId); set => base.PacketId = Convert.ToUInt16(value); }
    }
}
