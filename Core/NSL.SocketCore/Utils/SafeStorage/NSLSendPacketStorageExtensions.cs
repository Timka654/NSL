using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.SafeStorage
{
    public static class NSLSendPacketStorageExtensions
    {
        /// <summary>
        /// Регистрирует обработчик confirm-пакета для <see cref="NSLSendPacketStorage"/> в <paramref name="options"/>.
        /// Пакет с <paramref name="confirmPacketId"/> должен содержать uint32 tracking ID (первые 4 байта payload).
        ///
        /// Вызывать один раз при конфигурации клиента:
        /// <code>
        ///   options.AddNSLSendPacketStorage&lt;MyClient&gt;(confirmPacketId: 0xFF00);
        /// </code>
        /// Хранилище создаётся на каждого клиента отдельно через <see cref="GetOrCreateSendPacketStorage"/>.
        /// </summary>
        public static void AddNSLSendPacketStorage<TClient>(
            this CoreOptions options,
            ushort confirmPacketId,
            string objectBagKey = NSLSendPacketStorage.DefaultObjectBagKey)
            where TClient : BaseNetworkConnection
        {
            options.AddPacketHandle<TClient>(confirmPacketId, (client, data) =>
            {
                var id = NSLTrackedOutputPacketBuffer.ReadTrackingId(data);
                client.ObjectBag?.Get<NSLSendPacketStorage>(objectBagKey)?.Release(id);
            });
        }

        /// <summary>
        /// Возвращает <see cref="NSLSendPacketStorage"/> из ObjectBag клиента.
        /// Бросает, если ObjectBag не инициализирован или хранилище не зарегистрировано.
        /// </summary>
        public static NSLSendPacketStorage GetSendPacketStorage(
            this BaseNetworkConnection client,
            string objectBagKey = NSLSendPacketStorage.DefaultObjectBagKey)
            => client.ObjectBag.Get<NSLSendPacketStorage>(objectBagKey, throwIfNotExists: true);

        /// <summary>
        /// Возвращает существующее <see cref="NSLSendPacketStorage"/> или создаёт новое и кладёт в ObjectBag.
        /// ObjectBag должен быть инициализирован до вызова.
        /// </summary>
        public static NSLSendPacketStorage GetOrCreateSendPacketStorage(
            this BaseNetworkConnection client,
            string objectBagKey = NSLSendPacketStorage.DefaultObjectBagKey)
        {
            var existing = client.ObjectBag?.Get<NSLSendPacketStorage>(objectBagKey);
            if (existing != null)
                return existing;

            var storage = new NSLSendPacketStorage();
            client.ObjectBag.Set(objectBagKey, storage);
            return storage;
        }

        /// <summary>
        /// Отправляет confirm-пакет обратно отправителю.
        /// Вызывается серверным обработчиком после успешной обработки tracked-пакета:
        /// <code>
        ///   var trackId = NSLTrackedOutputPacketBuffer.ReadTrackingId(data);
        ///   // ... обработка ...
        ///   client.ConfirmTrackedPacket(trackId, confirmPacketId);
        /// </code>
        /// </summary>
        public static void ConfirmTrackedPacket(
            this BaseNetworkConnection client,
            uint trackingId,
            ushort confirmPacketId)
        {
            var packet = new OutputPacketBuffer { PacketId = confirmPacketId };
            packet.WriteUInt32(trackingId);
            client.Send(packet);
        }
    }
}
