using Logger;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DBEngine
{
    /// <summary>
    /// Хранилище отложенных запросов базы данных которые выполняються раз в определенное время
    /// </summary>
    public class DbCommandQueue
    {
        public delegate void ExecutedDbCommandDelegate(long index);

        public event ExecutedDbCommandDelegate ExecutedDbCommandEvent;

        protected DbConnectionPool connection_pool;

        private TimeSpan DelayCommandTime = TimeSpan.FromSeconds(20);

        private long CurrentIndex = 1;

        protected ILogger logger;

        protected ConcurrentDictionary<long, Action<DBCommand>> WaitList = new ConcurrentDictionary<long, Action<DBCommand>>();

        protected AutoResetEvent invoker_locker = new AutoResetEvent(true);

        public DbCommandQueue(DbConnectionPool connection_pool, ILogger logger)
        {
            this.connection_pool = connection_pool;
            this.logger = logger;

            FlushWhile();
        }

        /// <summary>
        /// Добавить отложенный запрос
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Индекс в хранилище</returns>
        public long AppendCommand(Action<DBCommand> command)
        {
            long index = Interlocked.Increment(ref CurrentIndex);

            if (index == long.MaxValue - 10000)
                CurrentIndex = 1;

            WaitList.TryAdd(index, command);

            return index;
        }

        /// <summary>
        /// Установка времени ожидания для дальнейших запросов
        /// </summary>
        /// <param name="delay"></param>
        public void SetDelayTime(TimeSpan delay)
        {
            DelayCommandTime = delay;
        }

        private async void FlushWhile()
        {
            while (true)
            {
                await Task.Delay(DelayCommandTime);
                try
                {
                    invoker_locker.WaitOne();
                    foreach (var item in WaitList)
                    {
                        item.Value.Invoke(connection_pool.GetCommand());

                        ExecutedDbCommandEvent?.Invoke(item.Key);

                        WaitList.TryRemove(item.Key, out var temp);
                    }
                    invoker_locker.Set();
                }
                catch (Exception ex)
                {
                    logger.Append(LoggerLevel.Error, ex.ToString());
                }
            }
        }
    }
}
