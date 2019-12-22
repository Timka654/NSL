using BinarySerializer;
using SocketCore.Utils.Buffer;
using System;

namespace SocketCore.Extensions.BinarySerializer
{
    public static class IOExtensions
    {
        public static T Deserialize<T>(this InputPacketBuffer _this, string schemeName)
        {
            global::BinarySerializer.BinarySerializer bs = new global::BinarySerializer.BinarySerializer(TypeStorage.Instance);

            int offset = _this.Offset;

            var result = bs.Deserialize<T>(schemeName, _this.GetBuffer(), ref offset);

            _this.Seek(offset, System.IO.SeekOrigin.Begin);

            return (T)result;
        }

        public static void Serialize(this OutputPacketBuffer _this, object obj, string schemeName)
        {
            global::BinarySerializer.BinarySerializer bs = new global::BinarySerializer.BinarySerializer(TypeStorage.Instance);
            var buf = bs.Serialize(schemeName, obj);
            _this.Write(buf, 0, buf.Length);
        }

        public static void Serialize<T>(this OutputPacketBuffer _this, T obj, string schemeName)
        {
            global::BinarySerializer.BinarySerializer bs = new global::BinarySerializer.BinarySerializer(TypeStorage.Instance);
            var r = bs.Serialize<T>(schemeName, obj);
            _this.Write(r, 0, r.Length);
        }
    }
}
