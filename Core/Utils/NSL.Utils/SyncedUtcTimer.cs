using System;
using System.Threading;

namespace NSL.Utils
{
    public class SyncedUtcTimer : IDisposable
    {
        private readonly TimeSpan _interval;
        private readonly NtpTimeProvider _timeProvider;

        private readonly Timer _timer;
        private bool _isDisposed;

        public event EventHandler<DateTime> OnSyncedTick = (d, t) => { };

        /// <param name="interval">Интервал срабатывания таймера</param>
        /// <param name="timeProvider">Инстанс провайдера. Если null, берется Singleton</param>
        public SyncedUtcTimer(
            TimeSpan interval,
            NtpTimeProvider timeProvider = null)
        {
            _interval = interval;
            _timeProvider = timeProvider ?? NtpTimeProvider.Instance;

            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            ScheduleNextTick();
        }

        private void ScheduleNextTick()
        {
            if (_isDisposed) return;

            // Запрашиваем обновление в фоне (провайдер сам решит, пора ли)
            _ = _timeProvider.TryRefreshAsync();

            DateTime now = _timeProvider.SyncedNow;

            // Вычисляем время до следующей ровной границы интервала
            long intervalTicks = _interval.Ticks;
            long nextTickTicks = ((now.Ticks / intervalTicks) + 1) * intervalTicks;

            long delayTicks = nextTickTicks - now.Ticks;
            int delayMs = (int)TimeSpan.FromTicks(delayTicks).TotalMilliseconds;

            if (delayMs <= 0) delayMs = 1;

            _timer.Change(delayMs, Timeout.Infinite);
        }

        private void TimerCallback(object state)
        {
            if (_isDisposed) return;

            try
            {
                OnSyncedTick(this, _timeProvider.SyncedNow);
            }
            catch
            {
                // Глушим ошибки подписчиков
            }

            ScheduleNextTick();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
        }
    }
}