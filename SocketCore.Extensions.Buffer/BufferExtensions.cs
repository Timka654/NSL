using Newtonsoft.Json;
using SocketCore.Utils.Buffer;
using System;

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
    }
}
