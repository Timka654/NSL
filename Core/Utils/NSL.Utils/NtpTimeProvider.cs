using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Utils
{
    public class NtpTimeProvider
    {
        private static NtpTimeProvider _instance;

        /// <summary>
        /// Default singleton instance with 30 minutes sync interval. You can create your own instances if you need different intervals or NTP servers.
        /// </summary>
        public static NtpTimeProvider Instance => _instance ?? (_instance = new NtpTimeProvider(TimeSpan.FromMinutes(30)));

        public static DateTime UtcNow => Instance.SyncedNow;

        private TimeSpan _ntpOffset = TimeSpan.Zero;
        private DateTime _lastSyncUtc = DateTime.MinValue;
        private readonly TimeSpan _syncReloadTime;
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);
        private readonly string _ntpServer;

        /// <param name="syncReloadTime">Как часто этот таймер будет "просить" обновить NTP</param>
        public NtpTimeProvider(
            TimeSpan syncReloadTime,
            string ntpServer = "pool.ntp.org")
        {
            _syncReloadTime = syncReloadTime;
            _ntpServer = ntpServer;
        }

        // Отдает текущее синхронизированное время
        public DateTime SyncedNow => DateTime.UtcNow + _ntpOffset;

        /// <summary>
        /// Пытается обновить время, если прошел заданный интервал.
        /// Блокирует одновременные запросы от сотен таймеров.
        /// </summary>
        public async Task TryRefreshAsync()
        {
            // Быстрая проверка без лока
            if (DateTime.UtcNow - _lastSyncUtc < _syncReloadTime)
                return;

            // Если кто-то уже обновляет время прямо сейчас — просто уходим (не ждем)
            if (!await _syncLock.WaitAsync(0))
                return;

            try
            {
                // Double-check внутри лока
                if (DateTime.UtcNow - _lastSyncUtc < _syncReloadTime)
                    return;

                await FetchNtpTimeAsync();
                _lastSyncUtc = DateTime.UtcNow;
            }
            catch
            {
                // Молча глотаем ошибку сети. Время просто не обновится, 
                // таймеры продолжат работать на старом offset'е.
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task ForceRefreshAsync()
        {
            await _syncLock.WaitAsync();

            try
            {
                await FetchNtpTimeAsync();
                _lastSyncUtc = DateTime.UtcNow;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private async Task FetchNtpTimeAsync()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // NTP Version 3, Client

            var addresses = await Dns.GetHostAddressesAsync(_ntpServer);
            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.ReceiveTimeout = 3000;

                await socket.ConnectAsync(ipEndPoint);

                // T1:
                DateTime t1 = DateTime.UtcNow;

                await socket.SendAsync(new ArraySegment<byte>(ntpData), SocketFlags.None);
                var receiveArgs = new ArraySegment<byte>(ntpData);
                await socket.ReceiveAsync(receiveArgs, SocketFlags.None);

                // T4:
                DateTime t4 = DateTime.UtcNow;

                // T2:
                DateTime t2 = ParseNtpTimestamp(ntpData, 32);

                // T3:
                DateTime t3 = ParseNtpTimestamp(ntpData, 40);

                //  RFC 5905
                TimeSpan offset = TimeSpan.FromTicks(((t2 - t1).Ticks + (t3 - t4).Ticks) / 2);

                _ntpOffset = offset;
            }
        }

        private DateTime ParseNtpTimestamp(byte[] ntpData, int startIndex)
        {
            ulong intPart = BitConverter.ToUInt32(new[] {
        ntpData[startIndex + 3],
        ntpData[startIndex + 2],
        ntpData[startIndex + 1],
        ntpData[startIndex] }, 0);

            ulong fractPart = BitConverter.ToUInt32(new[] {
        ntpData[startIndex + 7],
        ntpData[startIndex + 6],
        ntpData[startIndex + 5],
        ntpData[startIndex + 4] }, 0);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);
        }
    }
}