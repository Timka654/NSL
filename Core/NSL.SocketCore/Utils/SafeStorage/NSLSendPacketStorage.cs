using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NSL.SocketCore.Utils.SafeStorage
{
    /// <summary>
    /// Хранилище для пакетов, требующих подтверждения доставки и обработки.
    /// 
    /// Жизненный цикл пакета:
    ///   1. <see cref="CreateTracked"/> — создать пакет с уникальным tracking ID;
    ///   2. <see cref="Send"/> — сохранить скомпилированные байты и отправить;
    ///   3. Сервер вызывает <c>ConfirmTrackedPacket</c> → клиент получает подтверждение;
    ///   4. <see cref="Release"/> — пакет убирается из хранилища.
    ///
    /// При реконнекте программист явно вызывает <see cref="Replay"/> когда считает нужным.
    ///
    /// Хранилище живёт в <see cref="NSL.SocketCore.Utils.ClientObjectBag"/> клиента.
    /// Ключ по умолчанию: <see cref="DefaultObjectBagKey"/>.
    /// </summary>
    public class NSLSendPacketStorage
    {
        public const string DefaultObjectBagKey = NSLObjectBagKeys.SendPacketStorage;

        private readonly ConcurrentDictionary<uint, byte[]> _pending = new ConcurrentDictionary<uint, byte[]>();
        private int _seq;

        /// <summary>
        /// Создаёт новый <see cref="NSLTrackedOutputPacketBuffer"/> с уникальным tracking ID.
        /// После заполнения данными — передать в <see cref="Send"/>.
        /// </summary>
        public NSLTrackedOutputPacketBuffer CreateTracked(ushort packetId)
        {
            var id = (uint)Interlocked.Increment(ref _seq);
            return NSLTrackedOutputPacketBuffer.Create(packetId, id);
        }

        /// <summary>
        /// Сохраняет скомпилированные байты пакета и отправляет его клиенту.
        /// Пакет остаётся в хранилище до вызова <see cref="Release"/>.
        /// </summary>
        public void Send(BaseNetworkConnection client, NSLTrackedOutputPacketBuffer packet, bool disposeOnSend = true)
        {
            var compiled = packet.CompilePacket();
            _pending[packet.TrackingId] = compiled;

            if (disposeOnSend)
                packet.Dispose();

            client.Send(compiled, 0, compiled.Length);
        }

        /// <summary>
        /// Подтверждает доставку и обработку пакета — убирает из хранилища.
        /// Вызывается автоматически при получении confirm-пакета.
        /// </summary>
        public bool Release(uint trackingId)
            => _pending.TryRemove(trackingId, out _);

        /// <summary>
        /// Повторно отправляет все неподтверждённые пакеты.
        /// Вызывается программистом вручную — например после успешного реконнекта/session recovery.
        /// </summary>
        public void Replay(BaseNetworkConnection client)
        {
            foreach (var kvp in _pending.ToArray())
                client.Send(kvp.Value, 0, kvp.Value.Length);
        }

        /// <summary>Сбросить хранилище без повторной отправки (например при истечении сессии).</summary>
        public void Clear() => _pending.Clear();

        /// <summary>Количество неподтверждённых пакетов.</summary>
        public int PendingCount => _pending.Count;
    }
}
