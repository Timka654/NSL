using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.DBEngine
{
    /// <summary>
    /// Хранилище отложенных запросов базы данных которые выполняються раз в определенное время
    /// </summary>
    public class DbCommandQueue
    {
        protected DbConnectionPool connection_pool;

        private TimeSpan DelayCommandTime = TimeSpan.FromSeconds(20);

        private long CurrentIndex = 1;

        public Action<Exception> OnException { protected get; set; } = (e) => { };

        protected ConcurrentQueue<Action<DBCommand>> WaitList = new ConcurrentQueue<Action<DBCommand>>();


        protected AutoResetEvent invoker_locker = new AutoResetEvent(true);

        public DbCommandQueue(DbConnectionPool connection_pool)
        {
            this.connection_pool = connection_pool;

            FlushWhile();
        }

        /// <summary>
        /// Добавить отложенный запрос
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Индекс в хранилище</returns>
        public void AppendCommand(Action<DBCommand> command)
        {
            //long index = Interlocked.Increment(ref CurrentIndex);

            //if (index == long.MaxValue - 10000)
            //    CurrentIndex = 1;

            //WaitList.TryAdd(index, command);

            //return index;

            WaitList.Enqueue(command);
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
                invoker_locker.WaitOne();
                Execute();
                invoker_locker.Set();
            }
        }

        protected virtual void Execute()
        {
            ExecuteQueue(WaitList);
        }

        protected bool ExecuteQueue(ConcurrentQueue<Action<DBCommand>> commands)
        {
            DBEngine.DBCommand command = default;

            try
            {
                while (commands.TryDequeue(out var cmd))
                {
                    command = connection_pool.GetCommand();
                    cmd(command);

                    //ExecutedDbCommandEvent?.Invoke(item.Key);

                    command.CloseConnection();
                }

                return true;
            }
            catch (Exception ex)
            {
                command.CloseConnection();
                OnException(ex);

                return false;
            }
        }
    }
}
