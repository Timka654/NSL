using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.SafeStorage
{
    /// <summary>
    /// OutputPacketBuffer с вшитым tracking ID (uint32) в начале payload.
    /// Используется совместно с <see cref="NSLSendPacketStorage"/>.
    /// Сервер читает ID через <see cref="ReadTrackingId"/> и возвращает подтверждение.
    /// </summary>
    public class NSLTrackedOutputPacketBuffer : OutputPacketBuffer
    {
        public uint TrackingId { get; private set; }

        private NSLTrackedOutputPacketBuffer() { }

        internal static NSLTrackedOutputPacketBuffer Create(ushort packetId, uint trackingId)
        {
            var buf = new NSLTrackedOutputPacketBuffer
            {
                PacketId = packetId,
                TrackingId = trackingId
            };
            buf.WriteUInt32(trackingId);
            return buf;
        }

        /// <summary>
        /// Читает tracking ID из входящего пакета (первые 4 байта payload).
        /// Продвигает позицию чтения — последующие Read* получают основные данные.
        /// </summary>
        public static uint ReadTrackingId(InputPacketBuffer data)
            => data.ReadUInt32();
    }
}
